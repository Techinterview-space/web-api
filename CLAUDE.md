# CLAUDE.md - AI Agent Guidelines for Tech.Interviewer Web API

## Project Overview

This repository contains the backend API for [techinterview.space](https://techinterview.space) - a monolithic .NET 10 web API application.

**Repository:** https://github.com/techinterview-space/web-api

---

## Tech Stack

- .NET 10
- Entity Framework Core
- PostgreSQL (via Docker)
- Elasticsearch (via Docker)
- LocalStack (AWS services emulation)

---

## Code Style Guidelines

### Ruleset File

**ALWAYS follow the linter rules defined in `src/standard.ruleset`.**

Before writing any code, review the `standard.ruleset` file to understand the project's code analysis rules. This file contains the authoritative style and quality rules for the project.

```bash
# Location of the ruleset file
src/standard.ruleset
```

### Additional Style Guidelines

In addition to the ruleset, follow these conventions consistent with the existing codebase:

#### Naming Conventions

| Element | Style | Example |
|---------|-------|---------|
| Namespaces | PascalCase | `TechInterviewer.Features.Interviews` |
| Classes | PascalCase | `InterviewService` |
| Interfaces | PascalCase with "I" prefix | `IInterviewRepository` |
| Methods | PascalCase | `GetInterviewByIdAsync` |
| Properties | PascalCase | `CreatedAt` |
| Private fields | camelCase with underscore prefix | `_dbContext` |
| Local variables | camelCase | `interviewId` |
| Parameters | camelCase | `cancellationToken` |
| Async methods | PascalCase with "Async" suffix | `CreateInterviewAsync` |

#### Code Organization

- Follow existing project structure and patterns
- Use dependency injection for all service dependencies
- Use `async`/`await` for all asynchronous operations
- Follow existing patterns for controllers, services, and repositories

---

## Build Verification

### Required Action After Code Changes

**After applying any feature or code modification, always run:**

```bash
cd src
dotnet build
```

This ensures:
- All syntax errors are caught
- All references are resolved
- Code analysis rules from `standard.ruleset` pass
- The solution compiles successfully

If the build fails, fix the issues before considering the task complete.

---

## Database Migrations

### ⚠️ IMPORTANT: Migration Restrictions

**DO NOT generate or modify Entity Framework migrations.**

- Migrations will be created manually by the user using the provided PowerShell script or EF CLI
- **NEVER** touch or modify `*DatabaseContextModelSnapshot.cs` files (DbContext snapshots)
- **NEVER** create new migration files in the `Migrations` folder
- **NEVER** modify existing migration files

When making changes to entity models:
1. Modify only the entity classes as needed
2. Inform the user that migrations need to be generated manually
3. Remind the user to run `dotnet ef migrations add <MigrationName>`

---

## Running the Application

### Prerequisites

1. Docker Desktop installed and running
2. .NET 10 SDK installed

### Start Infrastructure

```bash
# Start required services (database, elasticsearch, localstack)
docker-compose up -d --build database.api elasticsearch localstack
```

### Run the Application

```bash
cd src
dotnet run
```

The application will be available at `https://localhost:5001`

---

## Unit Testing Guidelines

### Project Structure

**DO NOT create new test projects.** Test projects will be created manually by the user.

When asked to write tests, add test files to the existing test project structure.

### Testing Framework

All unit tests must use **xUnit** library.

### Test File Requirements

Each test file **MUST** contain at least one test for the **Happy Path** scenario.

Additional tests are optional but recommended for:
- Edge cases
- Error handling
- Boundary conditions
- Invalid input validation

### Test Naming Convention

Use descriptive test names following this pattern:

```csharp
[Fact]
public async Task MethodName_StateUnderTest_ExpectedBehavior()
{
    // Arrange
    // Act
    // Assert
}
```

Examples:
```csharp
[Fact]
public async Task GetInterview_WithValidId_ReturnsInterview()

[Fact]
public async Task GetInterview_WithInvalidId_ThrowsNotFoundException()

[Theory]
[InlineData(null)]
[InlineData("")]
public async Task CreateInterview_WithInvalidTitle_ThrowsValidationException(string invalidTitle)
```

### Test Structure

Follow the **Arrange-Act-Assert** pattern:

```csharp
[Fact]
public async Task CreateInterview_WithValidData_ReturnsCreatedInterview()
{
    // Arrange
    var request = new CreateInterviewRequest { /* ... */ };
    
    // Act
    var result = await _sut.CreateInterviewAsync(request, CancellationToken.None);
    
    // Assert
    Assert.NotNull(result);
    Assert.Equal(expected, result.Id);
}
```

---

## Test Framework Utilities

If the project contains a test framework, **use the existing utilities**:

### Available Test Utilities (if present)

#### Mock Services
Look for classes that mock business services:
- Pattern: `Mock*Service`, `*ServiceMock`, `Fake*Service`, `*ServiceFake`
- Location: Usually in a `Mocks/` or `Fakes/` folder within the test project

#### Entity Fakes
Look for `...Fake` classes that create test database entities:
- Pattern: `*Fake`, `*Builder`
- Purpose: Create valid entity instances for testing
- Example usage:
  ```csharp
  var interview = new InterviewFake()
      .WithUserId(userId)
      .WithStatus(InterviewStatus.Completed)
      .Please(context);
  ```

#### In-Memory Database Context
Look for `InMemoryDatabaseContext` or similar classes for database operations in tests:
- Use for integration-style tests that need database operations
- Provides isolated, in-memory database for each test
- Example usage:
  ```csharp
  using var context = new InMemoryDatabaseContext();
  context.Interviews.Add(interviewEntity);
  await context.SaveChangesAsync();
  // ... test repository methods
  ```

### Discovering Test Utilities

Before writing tests, check for existing test infrastructure:

```bash
# Find fake/mock classes
find . -name "*Fake*.cs" -o -name "*Mock*.cs" -o -name "*Builder*.cs"

# Find in-memory context
find . -name "*InMemory*.cs" -o -name "*TestDbContext*.cs"
```

---

## Project-Specific Notes

### This is a Monolith

This is a single monolithic application - there are no microservices or separate service projects. All features are contained within this single solution.

### Docker Compose Services

The project uses Docker Compose for local development infrastructure:
- `database.api` - PostgreSQL database
- `elasticsearch` - Search functionality
- `localstack` - AWS services emulation (S3, etc.)

### API Documentation

The API provides RSS feeds and standard REST endpoints. Check the README.md for API documentation.

---

## Summary Checklist

Before completing any task, verify:

- [ ] Code follows rules defined in `src/standard.ruleset`
- [ ] `dotnet build` succeeds without errors in the `src` directory
- [ ] No migrations were generated or modified
- [ ] No DbContext snapshot files were touched
- [ ] Unit tests use xUnit
- [ ] Each test file has at least one Happy Path test
- [ ] Existing test framework utilities are used when available
- [ ] No new test projects were created
- [ ] Code follows existing patterns in the codebase