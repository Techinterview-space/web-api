# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Backend API for [techinterview.space](https://techinterview.space) - a monolithic .NET 10 web API for interview management, salary data analysis, company reviews, and AI-powered insights.

**Repository:** https://github.com/techinterview-space/web-api

---

## Common Commands

### Build
```bash
cd src && dotnet build
```

### Run All Tests
```bash
cd src && dotnet test
```

### Run Single Test Project
```bash
cd src && dotnet test Domain.Tests
cd src && dotnet test InfrastructureTests
cd src && dotnet test Web.Api.Tests
```

### Run Single Test
```bash
cd src && dotnet test --filter "FullyQualifiedName~TestClassName.TestMethodName"
# Example:
cd src && dotnet test --filter "FullyQualifiedName~InterviewsControllerTests.Get_ReturnsOnlyMyInterviews"
```

### Run Application
```bash
# Start infrastructure first
docker-compose up -d --build database.api elasticsearch localstack

# Run the API
cd src && dotnet run --project Web.Api
```

**Application URL:** `https://localhost:5001`
**Swagger UI:** `https://localhost:5001/swagger`

---

## Architecture

### Layer Structure (Clean Architecture)
```
Web.Api (Controllers, Features, Middlewares)
    ↓
Infrastructure (Database, External Services, AI, Email, Storage)
    ↓
Domain (Entities, Value Objects, Validation, Enums)
```

### Solution Projects
- **Web.Api** - ASP.NET Core API, feature-based controllers
- **Domain** - Core entities, enums, validation, value objects
- **Infrastructure** - EF Core DbContext, external integrations, services
- **TestUtils** - Shared testing utilities (fakes, mocks, in-memory DB)
- **Domain.Tests**, **InfrastructureTests**, **Web.Api.Tests** - Test projects

### Key Design Patterns
- **CQRS-like**: `IRequestHandler<TRequest, TResult>` for command/query separation
- **Mediator**: `ServiceProviderExtensions.HandleBy<THandler, TRequest, TResult>()`
- **Feature-based organization**: Controllers grouped by domain (Interviews, Salaries, Companies, etc.)

---

## Key File Locations

| Component | Location |
|-----------|----------|
| Startup/DI | `src/Web.Api/Startup.cs`, `src/Web.Api/Setup/ServiceRegistration.cs` |
| Database Context | `src/Infrastructure/Database/DatabaseContext.cs` |
| Entity Configs | `src/Infrastructure/Database/Config/` |
| Code Analysis Rules | `src/standard.ruleset` |
| Feature Controllers | `src/Web.Api/Features/{FeatureName}/` |
| Test Fakes | `src/TestUtils/Fakes/` |
| In-Memory DB | `src/TestUtils/Db/InMemoryDatabaseContext.cs` |

---

## Code Style

**ALWAYS follow `src/standard.ruleset`** - StyleCop and CA rules enforced as errors.

### Naming Conventions
| Element | Style | Example |
|---------|-------|---------|
| Private fields | `_camelCase` | `_dbContext` |
| Async methods | `PascalCaseAsync` | `GetInterviewAsync` |
| Interfaces | `IPascalCase` | `IInterviewService` |
| Local variables/params | `camelCase` | `interviewId` |

---

## Database Migrations

**DO NOT generate or modify migrations.** Migrations are created manually by the user.

- Never modify `*DatabaseContextModelSnapshot.cs` files
- Never create/modify files in `src/Infrastructure/Migrations/`
- When changing entity models, inform user to run: `dotnet ef migrations add <Name>`

---

## Testing

### Framework
xUnit with Moq and Faker.Net

### Test Utilities (in TestUtils project)
```csharp
// Entity fakes - create test data
var user = await new UserFake(Role.Admin).PleaseAsync(context);
var interview = new InterviewFake(user).Please(context);

// In-memory database
using var context = new InMemoryDatabaseContext();

// Fake auth
var fakeAuth = new FakeAuth(user);
var fakeCurrentUser = new FakeCurrentUser(user);
```

### Test Naming Pattern
```csharp
[Fact]
public async Task MethodName_StateUnderTest_ExpectedBehavior()
```

Each test file must have at least one Happy Path test.

---

## Verification Checklist

Before completing any task:
- [ ] `cd src && dotnet build` succeeds
- [ ] No migrations generated or modified
- [ ] Code follows `standard.ruleset`
- [ ] Tests use xUnit and existing TestUtils utilities
