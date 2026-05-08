# Architecture

This service is a monolithic ASP.NET Core API. The runtime boundary is a single Kestrel process in a Docker container; everything else (database, search, object storage, email, AI, Telegram) is an external dependency reached over the network.

## Runtime topology

```
Frontend (Angular)
        │   HTTPS, JWT bearer
        ▼
┌─────────────────────────┐
│      Web.Api            │   ASP.NET Core, Kestrel, port 5001
│   Controllers + Mediator│
└──┬──────────────────────┘
   │ DI
   ▼
┌─────────────────────────┐         External
│    Infrastructure       │ ───►   • PostgreSQL  (Npgsql, EF Core)
│ EF Core, Auth, AI, Mail │ ───►   • Elasticsearch (Serilog sink)
│ Telegram, S3, Currencies│ ───►   • S3 / LocalStack
└──┬──────────────────────┘ ───►   • Resend (or local sender in Dev)
   │                         ───►   • Telegram Bot API (long-poll)
   ▼                         ───►   • OpenAI / Claude
┌─────────────────────────┐ ───►   • GitHub GraphQL
│        Domain           │ ───►   • National Bank of Kazakhstan currency XML
│  Entities + Validation  │
└─────────────────────────┘
```

`docs/interactions.md` lists every external system the service touches and which file owns the integration.

## Solution layout

The solution lives entirely under `src/`. The root holds infra, CI, and docs only.

| Project | Role |
|---|---|
| `src/Web.Api` | Composition root. Controllers, feature handlers, middleware, DI setup, hosted services, scheduler. |
| `src/Infrastructure` | Side-effectful code: `DatabaseContext`, OAuth providers, JWT, AI providers, email, Telegram, S3, currency feed. |
| `src/Domain` | Pure model: entities, value objects, validation, enums. References no I/O packages. |
| `src/TestUtils` | Shared test infrastructure (fakes, in-memory and SQLite contexts, fake auth). Referenced by all test projects. |
| `src/Domain.Tests`, `src/InfrastructureTests`, `src/Web.Api.Tests` | xUnit test projects, one per layer. |

`src/techinterview.sln` ties them together. `src/Directory.Build.props` enforces StyleCop and `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` outside of Visual Studio, plus the shared `standard.ruleset`.

## Composition

`Program.cs` builds a default web host and delegates to `Startup`. `Startup.ConfigureServices` chains a sequence of extension methods, each owning one subsystem:

- `SetupDatabase` — registers `DatabaseContext` against PostgreSQL and sets `Npgsql.EnableLegacyTimestampBehavior=true` (`src/Web.Api/Setup/DatabaseConfig.cs`).
- `SetupAppServices` — registers domain services, AI providers, OAuth providers, S3 storage, Telegram bot providers (`src/Web.Api/Setup/ServiceRegistration.cs`).
- `SetupEmailIntegration` — picks `LocalEmailApiSender` in Development, `ResendEmailApiSender` otherwise.
- `SetupHealthCheck` — adds `DatabaseHealthCheck` and exposes `GET /health` with a custom JSON writer (`src/Web.Api/Setup/Healthcheck/`).
- `SetupAuthentication` — JWT bearer with internal HMAC key when `OAuth:Jwt:Secret` is set, otherwise external IdentityServer authority (`src/Web.Api/Setup/Auth.cs`).
- `SetupScheduler` — Coravel scheduler and recurring jobs (`src/Web.Api/Setup/ScheduleConfig.cs`).
- `RegisterAllImplementations(IRequestHandler<,>)` — assembly scan that registers every closed `IRequestHandler<TRequest, TResult>` as `Scoped`. Manual handler registration is unnecessary and discouraged.

The pipeline (`Startup.Configure`) is conventional: correlation id → exception → logging → Swagger → HTTPS redirect → CORS → session → AuthN/AuthZ → routing → endpoints → 404 page.

## Mediator pattern

`Infrastructure.Services.Mediator.IRequestHandler<TRequest, TResult>` is the only abstraction; there is no MediatR. Controllers dispatch through `IServiceProvider.HandleBy<THandler, TRequest, TResult>(...)`. Some handlers are also wired in via `[FromServices]` directly. Handlers are registered automatically by the scan above; do not edit DI when adding a new one.

## Feature folders

`src/Web.Api/Features/` is organised by domain area, not by HTTP verb. Each folder contains a controller plus per-action subfolders holding the handler, request, response, and DTOs for that action. Examples: `Features/Auth/`, `Features/Salaries/`, `Features/Companies/`, `Features/Interviews/`, `Features/PublicSurveys/`, `Features/CompanyReviewsSubscriptions/`. `Features/BackgroundJobs/` holds Coravel `IInvocable` jobs scheduled in `ScheduleConfig.cs`.

Browse `src/Web.Api/Features/` for the full set; do not maintain a list here.

## Hosted services and scheduler

Two long-running components run inside the API process:

- `AppInitializeService` (`src/Web.Api/Setup/HostedServices/AppInitializeService.cs`) — on `StartAsync`: applies pending EF Core migrations, then starts both Telegram bots' long-poll loops (`SalariesTelegramBotHostedService`, `GithubProfileBotHostedService`).
- Coravel scheduler (`ScheduleConfig.Use`) — recurring jobs (currency refetch, salary stats, AI weekly analyses, channel-stats monthly aggregation, salary-update reminder emails staggered hourly). Each schedule uses `PreventOverlapping`. When a debugger is attached, several jobs also run every minute for manual testing.

## Persistence

PostgreSQL via `Npgsql.EntityFrameworkCore.PostgreSQL`. Entity configuration lives in `src/Infrastructure/Database/Config/` and is applied via `ApplyConfigurationsFromAssembly` in `OnModelCreating`. Migrations live in `src/Infrastructure/Migrations/` and are authored by hand — agents must not generate or modify them.

`DatabaseContext.OnBeforeSaving` updates `IHasDates` timestamps automatically. The in-memory test context skips the create timestamp branch when one is already set so test fakes can pin dates.

## Logging

Serilog with the Elasticsearch sink. Configured in `src/Web.Api/Services/Logging/ElkSerilog.cs` via `_configuration.GetConnectionString("Elasticsearch")`. The `CorrelationIdMiddleware` injects a request id into the log scope for every request.

## Deployment

Deploys are triggered by pushes to `main`:

1. `.github/workflows/deploy.yml` `build` job — `sed`-substitutes secrets into `src/Web.Api/appsettings.Production.json`, builds the Docker image from `src/Dockerfile`, and pushes it to a DigitalOcean container registry.
2. `deploy-ssh` job — SCPs `docker-compose.deploy.yml` to the deploy host and runs `docker-compose pull && docker-compose up -d`.

The runtime image is `mcr.microsoft.com/dotnet/aspnet:10.0`. The container loads a self-signed `aspnetapp.pfx` and listens on `5001` (HTTPS) and `5000` (HTTP).

The `manifests/` directory contains Kubernetes deployment YAML referencing an `intranet` namespace and image placeholders (`{api-image}`, `{frontend-image}`). These are not used by the current deployment pipeline; the active path is the docker-compose flow above. Treat `manifests/` as legacy until the team confirms otherwise.
