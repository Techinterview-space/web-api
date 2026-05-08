# Testing

xUnit, Moq, Faker.Net. Three test projects, one per layer:

- `src/Domain.Tests` — pure domain behaviour and value objects.
- `src/InfrastructureTests` — EF configurations, services, integrations with side effects stubbed.
- `src/Web.Api.Tests` — controller and feature handler tests with the in-memory database.

All three reference `src/TestUtils` for shared helpers; `Web.Api.Tests` and the Infrastructure tests use it heavily.

## Running tests

```bash
cd src && dotnet test                                      # full suite
cd src && dotnet test Domain.Tests                          # one project
cd src && dotnet test --filter "FullyQualifiedName~ClassName.MethodName"
```

CI runs `dotnet test ./src/techinterview.sln --no-restore` on every PR (`.github/workflows/test.yml`) and uploads coverage to Codecov.

## Database in tests

Two test contexts in `src/TestUtils/Db/`:

- `InMemoryDatabaseContext` — EF Core in-memory provider, fresh database per instance. Use for the common case.
- `SqliteContext` — EF Core SQLite-in-memory provider. Use when behaviour depends on relational features (FK constraints, real indexes, transactions).

Both inherit from `Infrastructure.Database.DatabaseContext`, so production code paths run against them unmodified. **Do not** mock `DatabaseContext` directly — production has been bitten by mock/real divergence.

Both contexts call `Database.EnsureDeleted()` then `EnsureCreated()` in their constructor. `InMemoryDatabaseContext` overrides `IsInMemory()` so the `OnBeforeSaving` hook respects pre-set `CreatedAt` values from fakes.

## Test data: fakes

Fakes in `src/TestUtils/Fakes/` follow the builder-`Please` pattern. Every fake is an `IPlease<TEntity>`-shaped builder; call `.Please(context)` for a sync test or `.PleaseAsync(context)` for an async one.

```csharp
var user = await new UserFake(Role.Admin).PleaseAsync(context);
var interview = new InterviewFake(user).Please(context);
```

Fakes available out of the box: users, interviews, companies, company reviews, salaries, salary surveys, public surveys (and questions/options), subscriptions, currency collections, channel posts, monitored channels, and more — browse the directory.

## Auth in tests

`src/TestUtils/Auth/`:

- `FakeAuth` — implements `IAuthorization`; returns the user you constructed it with.
- `FakeCurrentUser` — `CurrentUser` value object pre-populated for the user.
- `FakeHttpContext` — minimal `IHttpContext` for handlers that read claims directly.

```csharp
var fakeAuth = new FakeAuth(user);
var fakeCurrentUser = new FakeCurrentUser(user);
```

## Service fakes and mocks

`src/TestUtils/Mocks/` and `src/TestUtils/Services/` cover currency, professions cache, CV storage, Telegram admin notifications, and the Telegram bot client provider. Prefer these over hand-rolled Moq setups; they exist because the real interactions are too coarse to mock at the call site.

## Naming and structure

- Test method names follow `MethodName_StateUnderTest_ExpectedBehavior`. Keep this pattern even if you find older tests that don't.
- Each test class is expected to have at least one happy-path test.
- Group tests by the entry point under test (e.g. one file per controller action or per handler).

## Caveats

- **CI .NET SDK mismatch** — `.github/workflows/test.yml` pins `dotnet-version: 9.0.100`, but the solution targets `net10.0` (`*.csproj`). The roll-forward usually picks up the installed `net10.0` runtime, but if CI fails with a target-framework error, the SDK pin is the first thing to update.
- **Build warnings are errors** — `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` is enforced outside Visual Studio (`src/Directory.Build.props`). StyleCop violations fail the build, which means they fail the test job.
- **Mocked database is forbidden** — the integration we care about is the EF Core query translation, not method dispatch.
