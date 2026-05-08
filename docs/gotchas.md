# Gotchas

Non-obvious behaviour that has bitten contributors. If you change anything in this file, also remove or update the corresponding warning in `CLAUDE.md`.

## Migrations are hand-authored

Do not generate or modify EF Core migrations. The convention is:

1. Change the entity model.
2. Tell the user to run `dotnet ef migrations add <Name>` themselves.
3. Never touch `*DatabaseContextModelSnapshot.cs`.
4. Never edit anything in `src/Infrastructure/Migrations/`.

`AppInitializeService.MigrateAsync` applies pending migrations on boot, so a generated migration will run on the next deploy.

## Guid primary keys must use `ValueGeneratedNever()`

Entities that set `Id = Guid.NewGuid()` in their constructor must call `ValueGeneratedNever()` in their EF entity configuration. Without it, EF Core's `ValueGeneratedOnAdd` convention causes entities added through navigation properties to be tracked as `Modified` instead of `Added`, producing `DbUpdateConcurrencyException` at `SaveChanges` time. The fix is in the configuration class, not the entity.

## Soft-delete is opt-in, not a query filter

There is no `HasQueryFilter` on `IHasDeletedAt`. Entities that implement it (`Company`, `User`, `PublicSurvey`, etc.) must be filtered with `.Active()` from `Infrastructure.Database.ContextExtensions`. Forgetting to do so silently returns deleted records.

## `AllAsync()` is `AsNoTracking`

`ContextExtensions.AllAsync<T>()` always appends `.AsNoTracking()`. Calling `context.Update()` on the returned entities does nothing useful — re-attach them with `Attach`/`Update` or fetch them tracked first.

## Handler registration is automatic

`RegisterAllImplementations` in `src/Web.Api/Setup/ServiceRegistration.cs` scans all loaded assemblies and registers every closed `IRequestHandler<TRequest, TResult>` with `Scoped` lifetime. There is no manual DI line for new handlers — adding one is unnecessary, and editing the scan is almost always wrong.

## `EnableLegacyTimestampBehavior` is global

`SetupDatabase` sets `AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true)`. Toggling this flips how `DateTime` and `DateTimeOffset` round-trip through PostgreSQL across the entire application. Do not change it.

## Build failures are lint failures

`src/Directory.Build.props` sets `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` outside Visual Studio, alongside StyleCop and `standard.ruleset`. If `dotnet build` fails, look for the analyzer rule before assuming a code defect.

## Production secrets are written by `sed`

`.github/workflows/deploy.yml` substitutes secrets directly into `src/Web.Api/appsettings.Production.json` at build time using `sed -i`. Three consequences:

- A new configuration key has to flow through both `appsettings.json` (with a `__PLACEHOLDER` value) and the workflow.
- Any change to the JSON shape that breaks the `sed` patterns will silently leave placeholders in production.
- Do not commit real secrets to `appsettings.Production.json`; commit placeholders only.

## CORS is wide open

`Startup.ConfigureServices` registers a CORS policy with `AllowAnyOrigin/Method/Header`. AuthN/AuthZ is the only access control. Do not add features that rely on origin checks.

## Two CI jobs, both named "Tests"

`.github/workflows/test.yml` and `.github/workflows/deploy.yml` both define a workflow named `Tests`. The second one is the deploy pipeline. When debugging CI, identify the workflow by file, not by name.

## CI uses .NET 9 SDK, solution targets net10.0

`.github/workflows/test.yml` pins `actions/setup-dotnet` at `9.0.100`, but every csproj targets `net10.0`. The runner has the `net10.0` runtime installed, so the test job works in practice via roll-forward. If CI fails with a missing-target-framework error, bump the SDK pin first.

## `frontend/` and `web-api/` are separate repos

The parent `techinterview/` directory is a workspace, not a Git repo. This repo (`web-api`) has its own remote at `github.com/techinterview-space/web-api`. Cross-stack changes need two PRs.

## Telegram bots are long-poll, in-process

`AppInitializeService` starts both Telegram bots' update loops on application start. They run inside the API process — there is no separate worker. Killing the API kills the bots; scaling the API horizontally would double-poll. There is webhook scaffolding only for monitored channels (`Features/ChannelStats/Webhook/`).

## Handler return types are not the controller's response shape

Handlers return DTOs that the controllers in `Features/*/` re-shape (or return as-is). Do not assume the wire shape from the handler signature; check the controller.
