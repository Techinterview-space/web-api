<!-- CRITICAL CONTEXT ANCHOR -->
<!-- The following block is the project's standing context anchor for agents. -->
<!-- Do not summarise, reword, or move it. It must be the first thing in this file. -->

# CRITICAL CONTEXT ANCHOR

You are working in a production .NET backend (`web-api`) for techinterview.space. Treat this repository as live infrastructure: changes ship to production from `main` via the SSH-based docker-compose pipeline in `.github/workflows/deploy.yml`.

Hard rules:
- Do not generate, edit, or delete EF Core migrations or `*DatabaseContextModelSnapshot.cs`. Migrations are authored by hand. If a model change requires one, say so and stop.
- Do not change `EnableLegacyTimestampBehavior` (set in `src/Web.Api/Setup/DatabaseConfig.cs`).
- Do not introduce mocks for the `DatabaseContext`. Use `InMemoryDatabaseContext` or `SqliteContext` from `src/TestUtils/Db/`.
- Build failures are lint failures: StyleCop and CA rules in `src/standard.ruleset` are enforced as errors via `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` in `src/Directory.Build.props`.
- The `RegisterAllImplementations` scan in `src/Web.Api/Setup/ServiceRegistration.cs` registers every `IRequestHandler<TRequest, TResult>` automatically — do not register handlers manually.
- For renames or signature changes, grep the entire `src/` tree, including `*Tests.cs` and the test projects (`Domain.Tests`, `InfrastructureTests`, `Web.Api.Tests`), before claiming the change is complete.
- Anything you would otherwise call `cd src/` is correct: the solution and all projects live under `src/`. The repo root holds infra, docs, and CI only.

<!-- END CRITICAL CONTEXT ANCHOR -->

## What this repo is

Backend API for [techinterview.space](https://techinterview.space). Monolithic ASP.NET Core service over PostgreSQL, with Elasticsearch (logs), S3 (assets), Resend (email), Telegram (two bots, polling), OpenAI/Claude (analysis), and the GitHub GraphQL API. Repository: https://github.com/techinterview-space/web-api.

## Working agreement

- Before any code change, summarise what you will change and what you will not, then proceed.
- When the user names a UI element, screen, or component ambiguously, ask for the exact file before editing.
- After modifying anything that has tests, run the full test suite for the affected project.
- Do not commit on the user's behalf. The user opens commits themselves.

The hard rules in the anchor above (migrations, `EnableLegacyTimestampBehavior`, `DatabaseContext` mocks, build-as-lint, mediator registration, cross-project grep, `cd src/`) take precedence over anything else and are not optional.

## Documentation maintenance (mandatory)

When you add a feature, change architecture, add/remove an integration, change auth or routes, add a new feature module, or introduce a new domain area, update the relevant docs in the **same change**:

| Change | Update |
|---|---|
| New/changed inbound endpoint, webhook, Telegram bot behaviour, or outbound HTTP client (OpenAI, Claude, GitHub GraphQL, Resend, NBK currency feed, S3/LocalStack, Elasticsearch sink, etc.) | [`docs/interactions.md`](docs/interactions.md) |
| New/changed feature folder under `src/Web.Api/Features/`, hosted service, Coravel-scheduled job, middleware, filter attribute, or composition-root wiring in `Setup/` | [`docs/architecture.md`](docs/architecture.md) |
| New top-level area under `src/Domain/Entities/`, renamed namespace, or new shared base type / value object in `Domain/` | [`docs/domain.md`](docs/domain.md) |
| New OAuth provider, JWT or M2M scope, role, or authorization filter attribute | [`docs/authentication.md`](docs/authentication.md) |
| New test context, fake, or shared helper under `src/TestUtils/` | [`docs/testing.md`](docs/testing.md) |
| New non-obvious constraint, EF Core trap, or production behaviour worth warning about | [`docs/gotchas.md`](docs/gotchas.md) |
| New env var or `appsettings*.json` key, new `docker-compose` service, or any quickstart change | [`README.md`](README.md) |

If you are unsure whether a doc update is needed, re-read the relevant section of [`docs/architecture.md`](docs/architecture.md) and the CRITICAL CONTEXT ANCHOR at the top of this file. Do not leave `docs/` in a stale state after a feature PR.

## Common commands

```bash
cd src && dotnet build
cd src && dotnet test
cd src && dotnet test Web.Api.Tests
cd src && dotnet test --filter "FullyQualifiedName~ClassName.MethodName"
```

To run the API locally:

```bash
docker-compose up -d --build database.api elasticsearch localstack
cd src && dotnet run --project Web.Api
```

App URL: `https://localhost:5001`. Swagger: `https://localhost:5001/swagger`. Health: `https://localhost:5001/health`. See `README.md` for full prerequisites.

## Where to look

| Need | Path |
|---|---|
| Architecture overview (runtime topology, project layout, hosted services, scheduler) | `docs/architecture.md` |
| Authentication (OAuth, JWT issuance, refresh tokens, M2M, role/scope filters) | `docs/authentication.md` |
| Domain aggregates and shared base types | `docs/domain.md` |
| External integrations (every system this service talks to and why) | `docs/interactions.md` |
| Test infrastructure (runners, fakes, in-memory vs SQLite contexts) | `docs/testing.md` |
| Non-obvious behaviour that has bitten contributors | `docs/gotchas.md` |
| Composition root | `src/Web.Api/Startup.cs`, `src/Web.Api/Setup/ServiceRegistration.cs` |
| Mediator pattern | `src/Infrastructure/Services/Mediator/` |
| EF DbContext | `src/Infrastructure/Database/DatabaseContext.cs` |
| Entity configurations | `src/Infrastructure/Database/Config/` |
| StyleCop ruleset | `src/standard.ruleset` |
| Feature controllers | `src/Web.Api/Features/<FeatureName>/` |

Naming conventions and analyzer rules live in `src/standard.ruleset` and `src/stylecop.json`. Read those instead of memorising them here.

## Mediator pattern

`Infrastructure.Services.Mediator.IRequestHandler<TRequest, TResult>` is the only abstraction. Handlers are registered automatically by `RegisterAllImplementations` in `src/Web.Api/Setup/ServiceRegistration.cs`; do not add manual DI lines for new handlers. Controllers dispatch via `IServiceProvider.HandleBy<THandler, TRequest, TResult>(...)` or `[FromServices]`.

## Tests

xUnit + Moq + Faker.Net. Use `InMemoryDatabaseContext` or `SqliteContext` from `src/TestUtils/Db/` — do not mock `DatabaseContext`. Entity fakes (`UserFake`, `InterviewFake`, etc.) live in `src/TestUtils/Fakes/`. See `docs/testing.md` for the full pattern.

## Gotchas (one-liners)

The full explanations live in `docs/gotchas.md`. The ones most likely to trip you up:

- Guid PKs set in constructors must be configured `ValueGeneratedNever()`.
- Soft-delete is manual: filter with `.Active()` from `ContextExtensions`.
- `AllAsync()` returns untracked entities.
- Production secrets are `sed`-substituted into `appsettings.Production.json` by CI.
- CI pins .NET SDK 9.0.100 even though projects target `net10.0`.
