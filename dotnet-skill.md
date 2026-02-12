# .NET Backend Best Practices Skill

Use this before writing any .NET backend code, before planning code changes and enhancements. This establishes style guidelines, architecture patterns, and teaches essential .NET techniques derived from a production-grade Clean Architecture project.

---

## Architecture

### Clean Architecture Layers

```
Web.Api (Presentation Layer)
  - Controllers, Features, Middlewares, Setup
  - References: Infrastructure, Domain
    ↓
Infrastructure (Infrastructure Layer)
  - Database (EF Core), External Services, AI, Email, Storage
  - References: Domain
    ↓
Domain (Core Layer)
  - Entities, Value Objects, Enums, Validation
  - References: Nothing (zero external dependencies)
```

**Rules:**
- Domain MUST NOT reference Infrastructure or Web.Api
- Infrastructure MUST NOT reference Web.Api
- Web.Api references both Infrastructure and Domain
- Keep Domain free from framework dependencies (no EF Core, no ASP.NET)

### Solution Structure

```
src/
├── Web.Api/                    # ASP.NET Core API host
│   ├── Features/               # Feature-based organization (controllers + handlers)
│   ├── Middlewares/             # HTTP pipeline middlewares
│   ├── Setup/                  # DI, Auth, Health, Attributes, HostedServices
│   └── Services/               # Application-specific services
├── Infrastructure/             # Data access, external integrations
│   ├── Database/               # DbContext, Migrations, Entity Configs
│   ├── Services/               # Mediator, Correlation, HTTP, etc.
│   └── Extensions/             # Query extension methods
├── Domain/                     # Core business logic
│   ├── Entities/               # Domain entities grouped by feature
│   ├── Enums/                  # Enumerations
│   ├── ValueObjects/           # Immutable value types
│   ├── Validation/             # Validators, guard clauses, exceptions
│   ├── Attributes/             # Custom validation attributes
│   └── Extensions/             # Pure utility extensions
├── TestUtils/                  # Shared test infrastructure
│   ├── Fakes/                  # Entity builders/fakes
│   ├── Db/                     # In-memory database context
│   └── Auth/                   # Fake authentication helpers
├── Domain.Tests/               # Domain layer unit tests
├── InfrastructureTests/        # Infrastructure layer tests
└── Web.Api.Tests/              # API layer / integration tests
```

---

## Feature-Based Organization (CQRS-lite)

### Pattern

Each feature is a folder containing its handler, command/query, and response types. Controllers dispatch to handlers via a lightweight mediator.

```
Features/
├── Salaries/
│   ├── SalariesController.cs
│   ├── AddSalary/
│   │   ├── AddSalaryCommand.cs
│   │   └── AddSalaryHandler.cs
│   ├── GetSalariesChart/
│   │   ├── GetSalariesChartQuery.cs
│   │   └── GetSalariesChartHandler.cs
│   └── Models/
│       └── UserSalaryDto.cs
```

### Mediator Interface

Define a simple request handler interface in the Infrastructure layer:

```csharp
namespace Infrastructure.Services.Mediator;

public interface IRequestHandler<TRequest, TResult>
{
    Task<TResult> Handle(
        TRequest request,
        CancellationToken cancellationToken);
}
```

### Mediator Dispatch Extension

```csharp
public static class ServiceProviderExtensions
{
    public static async Task<TResult> HandleBy<THandler, TRequest, TResult>(
        this IServiceProvider serviceProvider,
        TRequest request,
        CancellationToken cancellationToken = default)
        where THandler : IRequestHandler<TRequest, TResult>
    {
        var handler = serviceProvider.GetRequiredService<THandler>();
        return await handler.Handle(request, cancellationToken);
    }
}
```

### Auto-Registration of Handlers

Register all `IRequestHandler<,>` implementations at startup using assembly scanning:

```csharp
services.RegisterAllImplementations(
    typeof(IRequestHandler<,>));
```

Implementation:

```csharp
public static IServiceCollection RegisterAllImplementations(
    this IServiceCollection services,
    Type openGenericType,
    ServiceLifetime lifetime = ServiceLifetime.Scoped)
{
    var typesFromAssemblies = AppDomain.CurrentDomain
        .GetAssemblies()
        .Where(a => !a.IsDynamic)
        .SelectMany(a => a.GetTypes())
        .Where(t => !t.IsAbstract && !t.IsInterface)
        .Select(t => new
        {
            Implementation = t,
            Services = t.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == openGenericType)
        })
        .Where(x => x.Services.Any());

    foreach (var typeInfo in typesFromAssemblies)
    {
        foreach (var service in typeInfo.Services)
        {
            services.Add(new ServiceDescriptor(service, typeInfo.Implementation, lifetime));
            services.Add(new ServiceDescriptor(typeInfo.Implementation, typeInfo.Implementation, lifetime));
        }
    }

    return services;
}
```

### Controller Pattern

Controllers are thin dispatchers. They inject `IServiceProvider` and use `HandleBy`:

```csharp
[ApiController]
[Route("api/salaries")]
public class SalariesController : ControllerBase
{
    private readonly IServiceProvider _serviceProvider;

    public SalariesController(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    [HttpGet("chart")]
    public Task<SalariesChartResponse> GetChart(
        [FromQuery] GetSalariesChartQuery request,
        CancellationToken cancellationToken)
    {
        return _serviceProvider.HandleBy<GetSalariesChartHandler, GetSalariesChartQuery, SalariesChartResponse>(
            request,
            cancellationToken);
    }

    [HttpPost("")]
    [HasAnyRole]
    public async Task<CreateOrEditSalaryRecordResponse> Create(
        [FromBody] AddSalaryCommand request,
        CancellationToken cancellationToken)
    {
        return await _serviceProvider.HandleBy<AddSalaryHandler, AddSalaryCommand, CreateOrEditSalaryRecordResponse>(
            request,
            cancellationToken);
    }
}
```

### Handler Pattern

Handlers contain all business logic. They receive dependencies via constructor injection:

```csharp
public class AddSalaryHandler : IRequestHandler<AddSalaryCommand, CreateOrEditSalaryRecordResponse>
{
    private readonly IAuthorization _auth;
    private readonly DatabaseContext _context;

    public AddSalaryHandler(
        IAuthorization auth,
        DatabaseContext context)
    {
        _auth = auth;
        _context = context;
    }

    public async Task<CreateOrEditSalaryRecordResponse> Handle(
        AddSalaryCommand request,
        CancellationToken cancellationToken)
    {
        request.IsValidOrFail();

        var currentUser = await _auth.GetCurrentUserOrNullAsync(cancellationToken);
        // ... business logic ...

        var salary = await _context.SaveAsync(
            new UserSalary(/* ... */),
            cancellationToken);

        return CreateOrEditSalaryRecordResponse.Success(
            new UserSalaryDto(salary));
    }
}
```

### Command/Query Records

Use `record` types for commands and queries. Include self-validation:

```csharp
public record AddSalaryCommand : EditSalaryRequest
{
    public double Value { get; init; }
    public int Quarter { get; init; }
    public int Year { get; init; }
    public Currency Currency { get; init; }

    public override void IsValidOrFail()
    {
        base.IsValidOrFail();

        if (Value <= 0)
        {
            throw new BadRequestException("Value must be greater than 0");
        }

        if (Quarter is < 1 or > 4)
        {
            throw new BadRequestException("Quarter must be between 1 and 4");
        }
    }
}
```

---

## Domain Layer Patterns

### Entity Base Classes

Use a hierarchy of base classes for entities:

```csharp
// Audit timestamps interface
public interface IHasDates
{
    DateTimeOffset CreatedAt { get; }
    DateTimeOffset UpdatedAt { get; }
    void OnCreate(DateTimeOffset now);
    void OnUpdate(DateTimeOffset now);
}

// Abstract base with audit dates
public abstract class HasDatesBase : IHasDates
{
    public DateTimeOffset CreatedAt { get; protected set; }
    public DateTimeOffset UpdatedAt { get; protected set; }

    public virtual void OnCreate(DateTimeOffset now)
    {
        CreatedAt = UpdatedAt = now;
    }

    public virtual void OnUpdate(DateTimeOffset now)
    {
        UpdatedAt = now;
    }
}

// Generic ID interface
public interface IHasIdBase<out TKey>
    where TKey : struct
{
    TKey Id { get; }
}

// Standard long-based ID interface
public interface IHasId : IHasIdBase<long>
{
}

// Soft-delete interface
public interface IHasDeletedAt
{
    DateTimeOffset? DeletedAt { get; }
}

// Full base model: long Id + audit dates
public class BaseModel : HasDatesBase, IBaseModel
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public long Id { get; protected set; }

    public bool New()
    {
        return Id == default;
    }
}
```

**Key convention:** Properties use `protected set` to enforce encapsulation. State changes happen through explicit methods, not through public setters.

### Rich Domain Entities

Entities contain business logic. Mutations happen through methods, not setters:

```csharp
public class Company : HasDatesBase, IHasIdBase<Guid>
{
    public Guid Id { get; protected set; }
    public string Name { get; protected set; }
    public string NormalizedName { get; protected set; }
    // ...

    public Company(
        string name,
        string description,
        List<string> links,
        string logoUrl)
    {
        name = name?.Trim();
        Name = name ?? throw new ArgumentNullException(nameof(name));
        NormalizedName = name.ToUpperInvariant();
        // ... validation and initialization in constructor
    }

    public void AddReview(CompanyReview review)
    {
        NotDeletedOrFail();
        // ... business rules check ...
        Reviews.Add(review);
    }

    public void ApproveReview(Guid reviewId)
    {
        NotDeletedOrFail();
        var review = Reviews.FirstOrDefault(x => x.Id == reviewId)
            ?? throw new NotFoundException("Review not found");

        review.Approve(true);
        var newRating = RecalculateRating();
        RatingHistory.Add(new CompanyRatingHistory(newRating, this));
    }
}
```

### Soft Delete Pattern

```csharp
public static class ModelExtensions
{
    public static bool Active(this IHasDeletedAt deleted)
    {
        return deleted.DeletedAt == null;
    }

    public static T ActiveOrFail<T>(this T deleted)
        where T : IHasDeletedAt, IHasId
    {
        if (deleted.Active())
        {
            return deleted;
        }

        throw new InvalidOperationException(
            $"The entity of {typeof(T).Name} Id:{deleted.Id} has been deleted already");
    }
}
```

### Value Objects

Use `record` types for immutable value objects. Place in `Domain/ValueObjects/`:

```csharp
public record Pageable<T> : PaginatedListBase
{
    public IReadOnlyCollection<T> Results { get; protected set; }

    public Pageable(
        int currentPage,
        int pageSize,
        int totalItems,
        IReadOnlyCollection<T> results)
    {
        CurrentPage = currentPage;
        PageSize = pageSize;
        TotalItems = totalItems;
        Results = results;
    }
}

public record CurrentUser
{
    public string UserId { get; protected set; }
    public string Email { get; protected set; }
    public IReadOnlyCollection<Role> Roles { get; protected set; }

    public bool Has(Role role) => Roles.Contains(role);

    public void HasOrFail(Role role)
    {
        if (!Has(role))
        {
            throw new UnauthorizedAccessException($"User does not have {role} role");
        }
    }
}
```

---

## Validation & Exception Handling

### Guard Clause Extensions

Define fluent guard clauses as extension methods in `Domain/Validation/`:

```csharp
public static class ValidateUtilities
{
    public static T ThrowIfNull<T>(this T instance, string paramName, string customErrorMessage = null)
    {
        if (string.IsNullOrEmpty(paramName))
        {
            throw new InvalidOperationException("You should not pass null or empty string a paramName");
        }

        if (instance != null)
        {
            return instance;
        }

        throw customErrorMessage.NullOrEmpty()
            ? new ArgumentNullException(paramName: paramName)
            : new ArgumentNullException(paramName: paramName, message: customErrorMessage);
    }

    public static IReadOnlyCollection<T> ThrowIfNullOrEmpty<T>(
        this IReadOnlyCollection<T> collection, string paramName)
    {
        paramName.ThrowIfNullOrEmpty(nameof(paramName));
        collection.ThrowIfNull(paramName);

        if (!collection.Any())
        {
            throw new InvalidOperationException($"You should not pass empty collection: {paramName}");
        }

        return collection;
    }

    public static string ThrowIfNullOrEmpty(this string @string, string paramName)
    {
        if (@string.NullOrEmpty())
        {
            throw new BadRequestException("You should not pass null or empty string for " + paramName);
        }

        return @string;
    }
}
```

### Entity Validation via Data Annotations

Validate entities using `[ValidationAttribute]` and a generic validator:

```csharp
public class EntityValidator<T>
{
    private readonly T _entity;
    private readonly ICollection<string> _errors;

    public EntityValidator(T entity) { /* ... */ }

    public void ThrowIfInvalid()
    {
        if (!Valid())
        {
            throw EntityInvalidException.FromInstance<T>(_errors);
        }
    }
}

// Usage as extension:
public static T ThrowIfInvalid<T>(this T entity)
{
    new EntityValidator<T>(entity).ThrowIfInvalid();
    return entity;
}
```

### Custom Domain Exceptions

Define a hierarchy of domain exceptions that map to HTTP status codes:

```csharp
// 400 Bad Request - invalid client request
public class BadRequestException : InvalidOperationException
{
    public BadRequestException(string message) : base(message) { }
}

// 400 Bad Request - entity validation failed
public class EntityInvalidException : InvalidOperationException
{
    public static EntityInvalidException FromInstance<T>(ICollection<string> errors) { /* ... */ }
}

// 404 Not Found
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }

    // Factory methods for consistent messages
    public static NotFoundException CreateFromEntity<TEntity>(long id)
    {
        return new($"Did not find any {typeof(TEntity).Name} by id={id}");
    }

    public static NotFoundException CreateFromEntity<TEntity>(Guid id)
    {
        return new($"Did not find any {typeof(TEntity).Name} by id={id}");
    }
}

// 403 Forbidden
public class NoPermissionsException : Exception
{
    public NoPermissionsException(string message) : base(message) { }
    public NoPermissionsException() : this("The user has no permissions to do this operation") { }
}

// 422 Unprocessable Entity
public class InputValidationException : InvalidOperationException { /* ... */ }

// 409 Conflict
public class DbUpdateConcurrencyException : InvalidOperationException { /* ... */ }

// Wraps raw database errors
public class DatabaseException : InvalidOperationException { /* ... */ }
```

### Exception-to-HTTP Middleware

Map domain exceptions to HTTP status codes in middleware:

```csharp
public class ExceptionHttpMiddleware
{
    internal static readonly Dictionary<Type, int> StatusCodeTypes = new()
    {
        { typeof(UnauthorizedAccessException), StatusCodes.Status401Unauthorized },
        { typeof(AuthenticationException), StatusCodes.Status401Unauthorized },
        { typeof(NoPermissionsException), StatusCodes.Status403Forbidden },
        { typeof(NotFoundException), StatusCodes.Status404NotFound },
        { typeof(BadRequestException), StatusCodes.Status400BadRequest },
        { typeof(EntityInvalidException), StatusCodes.Status400BadRequest },
        { typeof(InputValidationException), StatusCodes.Status422UnprocessableEntity },
        { typeof(InvalidOperationException), StatusCodes.Status400BadRequest },
        { typeof(DbUpdateConcurrencyException), StatusCodes.Status409Conflict }
    };

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var statusCode = StatusCodes.Status500InternalServerError;
        string message = null;

        if (StatusCodeTypes.TryGetValue(exception.GetType(), out int status))
        {
            statusCode = status;
            message = exception.Message;
        }

        await WriteResponseAsync(context, statusCode, message, exception);
    }
}
```

---

## Infrastructure Patterns

### Database Context

Extend `DbContext` with helper methods for save operations and automatic audit timestamps:

```csharp
public class DatabaseContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<UserSalary> Salaries { get; set; }
    // ...

    // Save with entity validation
    public virtual async Task<TEntity> SaveAsync<TEntity>(
        TEntity entity,
        CancellationToken cancellationToken = default)
        where TEntity : class
    {
        entity.ThrowIfNull(nameof(entity));
        entity.ThrowIfInvalid();

        var entry = await AddAsync(entity, cancellationToken);
        await TrySaveChangesAsync(cancellationToken);

        return entry.Entity;
    }

    // Wrap SaveChanges to catch DB errors
    public async Task<int> TrySaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await SaveChangesAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            throw new DatabaseException(exception);
        }
    }

    // Transaction helper
    public async Task<TResult> DoWithinTransactionAsync<TResult>(
        Func<Task<TResult>> action,
        string errorMessage = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await Database.BeginTransactionAsync(cancellationToken);
            var result = await action();
            await Database.CommitTransactionAsync(cancellationToken);
            return result;
        }
        catch (Exception exception)
        {
            await Database.RollbackTransactionAsync(cancellationToken);
            throw new InvalidOperationException(errorMessage ?? "Cannot execute transaction", exception);
        }
    }

    // Automatic audit timestamps
    protected virtual void OnBeforeSaving()
    {
        var entries = ChangeTracker.Entries<IHasDates>();
        var currentDateTime = DateTimeOffset.Now;

        foreach (EntityEntry<IHasDates> entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Modified:
                    entry.Entity.OnUpdate(currentDateTime);
                    break;
                case EntityState.Added:
                    entry.Entity.OnCreate(currentDateTime);
                    break;
            }
        }
    }

    // Apply all entity configurations from assembly
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(DatabaseContext).Assembly);
    }
}
```

### Entity Type Configuration

Use `IEntityTypeConfiguration<T>` for each entity. Place in `Infrastructure/Database/Config/`:

```csharp
public class UserSalaryConfig : IEntityTypeConfiguration<UserSalary>
{
    public void Configure(EntityTypeBuilder<UserSalary> builder)
    {
        builder.ToTable("UserSalaries");
        builder.HasKey(x => x.Id);

        builder
            .HasOne(x => x.User)
            .WithMany(x => x.Salaries)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder
            .Property(x => x.UseInStats)
            .HasDefaultValue(true);
    }
}
```

### Query Extensions

Place reusable query logic as extension methods on `IQueryable<T>`:

```csharp
public static class UserExtensions
{
    public static Task<List<UserSalary>> GetUserRelevantSalariesAsync(
        this IQueryable<UserSalary> salaries,
        long userId,
        CancellationToken cancellationToken)
    {
        var currentQuarter = DateQuarter.Current;
        return salaries
            .Where(x => x.UserId == userId)
            .Where(x => x.Year == currentQuarter.Year || x.Year == currentQuarter.Year - 1)
            .AsNoTracking()
            .OrderByDescending(x => x.Year)
            .ThenByDescending(x => x.Quarter)
            .ToListAsync(cancellationToken);
    }
}
```

---

## Middleware Pipeline

Order matters. Configure in this sequence:

```csharp
public void Configure(IApplicationBuilder app)
{
    // 1. Correlation ID (first - ensures all logs have correlation)
    app.UseMiddleware<CorrelationIdMiddleware>();

    // 2. Exception handler (catches everything downstream)
    app.UseMiddleware<ExceptionHttpMiddleware>();

    // 3. Logging (logs exceptions, then re-throws for #2 to handle)
    app.UseLoggingMiddleware();

    // 4. Standard ASP.NET middleware
    app.UseSwagger();
    app.UseHttpsRedirection();
    app.UseRouting();
    app.UseCors(CorsPolicyName);
    app.UseSession();
    app.UseAuthentication();
    app.UseAuthorization();

    // 5. Health checks
    HealthCheckCustomResponse.Setup(app);

    // 6. Endpoints
    app.UseEndpoints(endpoints => endpoints.MapControllers());

    // 7. Catch-all 404 (last)
    app.UseMiddleware<DefaultNotFoundPageMiddleware>();
}
```

### Correlation ID Middleware

Propagate or generate a correlation ID per request:

```csharp
public class CorrelationIdMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = GetCorrelationId(context);
        context.Items.TryAdd(CorrelationIdAccessor.CorrelationIdHeaderName, correlationId);

        context.Response.OnStarting(() =>
        {
            context.Response.Headers.Append(
                CorrelationIdAccessor.CorrelationIdHeaderName, correlationId);
            return Task.CompletedTask;
        });

        await _nextInvoke(context);
    }
}
```

### Selective Logging Middleware

Log errors but skip expected exceptions:

```csharp
public class LoggingMiddleware
{
    private readonly IReadOnlyCollection<Type> _exceptionsToIgnore = new List<Type>
    {
        typeof(AuthenticationException),
        typeof(BadRequestException),
        typeof(EntityInvalidException),
        typeof(NotFoundException),
    };

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            if (!_exceptionsToIgnore.Contains(exception.GetType()))
            {
                _logger.LogError(exception, "Unhandled error. Message {Message}", exception.Message);
            }

            throw; // Re-throw for ExceptionHttpMiddleware to handle
        }
    }
}
```

---

## Authorization Patterns

### Custom Authorization Attribute

```csharp
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class HasAnyRoleAttribute : Attribute, IAuthorizationFilter
{
    public IReadOnlyCollection<Role> RolesToCheck { get; }

    public HasAnyRoleAttribute(params Role[] rolesToCheck)
    {
        RolesToCheck = rolesToCheck;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var hasAuth = context.HttpContext.User.Claims.Any();
        if (!hasAuth)
        {
            throw new AuthenticationException("You have to be authorized");
        }

        if (!RolesToCheck.Any() || new CurrentUser(context.HttpContext.User).HasAny(RolesToCheck))
        {
            return;
        }

        throw new NoPermissionsException("You are not allowed to interact with this action");
    }
}
```

Usage:
```csharp
[HasAnyRole]                        // Any authenticated user
[HasAnyRole(Role.Admin)]            // Admin only
[HasAnyRole(Role.Admin, Role.HR)]   // Admin or HR
```

---

## Dependency Injection

### Registration Pattern

Use extension methods on `IServiceCollection` grouped by concern:

```csharp
public static class ServiceRegistration
{
    public static IServiceCollection SetupAppServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHttpContextAccessor()
            .AddTransient<ICorrelationIdAccessor, CorrelationIdAccessor>()
            .AddScoped<IAuthorization, AuthorizationService>()
            .AddScoped<IGlobal, Global>()
            // ...
            ;

        return services;
    }
}
```

### Startup Composition

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services
        .SetupDatabase(_configuration, _environment)
        .SetupAppServices(_configuration)
        .SetupEmailIntegration(_environment, _configuration)
        .SetupHealthCheck(_configuration)
        .SetupAuthentication(_configuration)
        .SetupScheduler()
        .RegisterAllImplementations(typeof(IRequestHandler<,>));

    services.AddHostedService<AppInitializeService>();
}
```

### Lifetime Guidelines

| Lifetime | Use For |
|----------|---------|
| `Transient` | Stateless utilities, HTTP clients, lightweight services |
| `Scoped` | DbContext, Authorization, services that hold per-request state |
| `Singleton` | Configuration objects, PDF converters |

---

## Configuration

### Typed Configuration via Constructor

```csharp
public record Global : IGlobal
{
    public string AppName { get; }
    public string FrontendBaseUrl { get; }
    public string BackendBaseUrl { get; }

    public Global(IConfiguration configuration)
    {
        AppName = configuration["Global:AppName"];
        FrontendBaseUrl = configuration["Global:Frontend"];
        BackendBaseUrl = configuration["Global:Backend"];
    }

    public string InterviewWebLink(Guid id) => $"{FrontendBaseUrl}/interviews/{id}";
}
```

---

## Testing

### Framework

- **xUnit** for test framework
- **Moq** for mocking
- **Faker.Net** for test data generation
- In-memory EF Core database for integration tests

### Test Organization

Mirror the source structure:

```
Web.Api.Tests/Features/Salaries/AddSalary/AddSalaryHandlerTests.cs
Domain.Tests/ValueObjects/DateQuarterTests.cs
InfrastructureTests/Salaries/JobPostingParserTests.cs
```

### Test Naming Convention

```csharp
[Fact]
public async Task MethodName_StateUnderTest_ExpectedBehavior()

// Examples:
public async Task Handle_NoExistingRecordForDate_ValidData_CreatesNewRecord()
public async Task Handle_ExistingRecordForDate_ValidData_ThrowsBadRequestException()
public async Task GetUser_UserHimself_ReturnsSalaries()
```

Every test file MUST have at least one happy path test.

### Fake/Builder Pattern

Create fakes that inherit from domain entities:

```csharp
public class CurrenciesCollectionFake : CurrenciesCollection
{
    public CurrenciesCollectionFake(
        DateTime currencyDate,
        Dictionary<Currency, CurrencyContent> currencies = null)
        : base(currencies ?? CreateDefaultCurrencies(currencyDate))
    {
    }

    // Persist to in-memory DB
    public CurrenciesCollection Please(InMemoryDatabaseContext context)
    {
        var entry = context.Add((CurrenciesCollection)this);
        context.SaveChanges();
        return entry.Entity;
    }
}
```

Usage patterns:
```csharp
// Sync save
var company = new CompanyFake().Please(context);

// Async save
var user = await new UserFake(Role.Admin).PleaseAsync(context);
var salary = await new UserSalaryFake(user, 400_000).PleaseAsync(context);
```

### Integration Test Pattern

Test handlers directly with in-memory database:

```csharp
[Fact]
public async Task Handle_NoExistingRecordForDate_ValidData_CreatesNewRecord()
{
    // Arrange
    await using var context = new InMemoryDatabaseContext();
    var handler = new CreateCurrenciesCollectionHandler(context);

    var request = new CreateCurrenciesCollectionRequest
    {
        CurrencyDate = new DateTime(2024, 1, 15),
        Currencies = new Dictionary<Currency, double>
        {
            { Currency.USD, 450.50 },
            { Currency.EUR, 520.75 }
        }
    };

    // Act
    var result = await handler.Handle(request, CancellationToken.None);

    // Assert
    Assert.NotNull(result);
    Assert.Equal(2, result.Currencies.Count);

    var createdRecord = await context.CurrencyCollections
        .FirstOrDefaultAsync(x => x.CurrencyDate == request.CurrencyDate);
    Assert.NotNull(createdRecord);
}
```

### Controller Test Pattern

Test controllers using fakes for auth:

```csharp
[Fact]
public async Task GetUser_UserHimself_ReturnsSalaries()
{
    await using var context = new InMemoryDatabaseContext();
    var user = await new UserFake(Role.Interviewer).PleaseAsync(context);
    var salary = await new UserSalaryFake(user, 400_000).PleaseAsync(context);

    var controller = new UsersController(
        new FakeAuth(user),
        context);

    context.ChangeTracker.Clear();
    var result = await controller.GetUser(user.Id, default);
    Assert.Equal(user.Id, result.Id);
}
```

### Theory Tests

Use `[Theory]` with `[InlineData]` for parameterized tests:

```csharp
[Theory]
[InlineData("Text with salary 500000-800000", 500_000d, 800_000d)]
[InlineData("Salary from 200k", 200_000d, null)]
[InlineData("No salary info", null, null)]
public void GetResult_HasSalariesInText_Ok(
    string text,
    double? min,
    double? max)
{
    var result = new JobPostingParser(text).GetResult();
    Assert.Equal(min, result.MinSalary);
    Assert.Equal(max, result.MaxSalary);
}
```

---

## Code Style Rules

### StyleCop & Code Analysis

Enforce via `Directory.Build.props` with `standard.ruleset`. Warnings are treated as errors in CI:

```xml
<PropertyGroup Condition="'$(BuildingInsideVisualStudio)' != 'true'">
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
</PropertyGroup>
```

### stylecop.json

```json
{
    "settings": {
        "documentationRules": {
            "xmlHeader": false,
            "fileNamingConvention": "metadata"
        },
        "orderingRules": {
            "usingDirectivesPlacement": "outsideNamespace",
            "elementOrder": ["constant", "readonly"]
        },
        "layoutRules": {
            "newlineAtEndOfFile": "require"
        }
    }
}
```

### Naming Conventions

| Element | Convention | Example |
|---------|-----------|---------|
| Private fields | `_camelCase` | `_dbContext`, `_auth` |
| Async methods | `PascalCaseAsync` | `GetInterviewAsync` |
| Interfaces | `IPascalCase` | `IInterviewService` |
| Local variables/params | `camelCase` | `interviewId` |
| Constants | `PascalCase` | `CorsPolicyName` |
| File-scoped namespaces | Always | `namespace Domain.Entities;` |
| Records for DTOs | `PascalCase` | `record AuthTokenResponse` |

### Key StyleCop Rules (enforced as errors)

- **SA1101: None** - `this.` prefix NOT required
- **SA1309: None** - `_` prefix for fields IS allowed
- **SA1200: Error** - Using directives outside namespace
- **SA1201: Error** - Elements must appear in correct order
- **SA1402: Error** - One type per file
- **SA1500-SA1520: Error** - Braces and spacing rules
- **SA1600-SA1602: None** - XML documentation NOT required
- **SA1633-SA1652: None** - File headers NOT required

### Disabled Rules (intentionally relaxed)

- SA1101 (no `this.` prefix required)
- SA1118 (multiline parameters allowed)
- SA1305 (Hungarian notation check disabled)
- SA1309 (underscore prefix for fields allowed)
- SA1412 (encoding check disabled)
- SA1413 (trailing comma check disabled)
- SA1518 (end-of-file newline handling relaxed)
- SA1600-1602 (XML docs not required on all public types)
- SA1633-1652 (file headers not required)
- SA1649 (file name doesn't have to match first type)

---

## Database Migrations

**NEVER generate or modify migrations automatically.** Migrations are created manually.

- Never modify `*DatabaseContextModelSnapshot.cs` files
- Never create/modify files in `Infrastructure/Migrations/`
- When changing entity models, inform the developer to run: `dotnet ef migrations add <Name>`

---

## Response Patterns

### Success/Failure Response

```csharp
public record CreateOrEditSalaryRecordResponse
{
    public bool Success { get; init; }
    public string ErrorMessage { get; init; }
    public UserSalaryDto Salary { get; init; }

    public static CreateOrEditSalaryRecordResponse Success(UserSalaryDto salary) =>
        new() { Success = true, Salary = salary };

    public static CreateOrEditSalaryRecordResponse Failure(string message) =>
        new() { Success = false, ErrorMessage = message };
}
```

### DTO Pattern

Use records with `[JsonPropertyName]` when needed:

```csharp
public record AuthTokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; init; }

    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; init; }

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; init; }

    [JsonPropertyName("token_type")]
    public string TokenType { get; init; } = "Bearer";
}
```

---

## Background Services

### Base Class

```csharp
public abstract class BackgroundServiceBase : IHostedService, IDisposable
{
    private readonly CancellationTokenSource _stoppingCts = new();
    private Task _executingTask;

    protected abstract Task ExecuteAsync(CancellationToken stoppingToken);

    public virtual Task StartAsync(CancellationToken cancellationToken)
    {
        _executingTask = ExecuteAsync(_stoppingCts.Token);
        return _executingTask.IsCompleted ? _executingTask : Task.CompletedTask;
    }

    public virtual async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_executingTask == null) return;
        try
        {
            await _stoppingCts.CancelAsync();
        }
        finally
        {
            await Task.WhenAny(_executingTask, Task.Delay(Timeout.Infinite, cancellationToken));
        }
    }

    public void Dispose()
    {
        _stoppingCts.Cancel();
        _stoppingCts.Dispose();
        GC.SuppressFinalize(this);
    }
}
```

---

## Custom Validation Attributes

Place in `Domain/Attributes/` for reusable validation logic:

```csharp
public class NotDefaultValueAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value == null) return ValidationResult.Success;

        var type = value.GetType();
        if (type.IsValueType)
        {
            var defaultValue = Activator.CreateInstance(type);
            return !value.Equals(defaultValue)
                ? ValidationResult.Success
                : new ValidationResult(FormatError(validationContext));
        }

        return ValidationResult.Success;
    }
}
```

---

## Verification Checklist

Before completing any task:

- [ ] `dotnet build` succeeds with zero warnings
- [ ] All existing tests pass (`dotnet test`)
- [ ] New code has tests (at least one happy path)
- [ ] No migrations generated or modified
- [ ] Code follows `standard.ruleset` (StyleCop enforced)
- [ ] Entities use `protected set` for properties
- [ ] Business logic lives in entities or handlers, not controllers
- [ ] Domain exceptions used instead of returning error codes
- [ ] `CancellationToken` passed through async call chains
- [ ] Guard clauses used for parameter validation (`ThrowIfNull`, `ThrowIfNullOrEmpty`)
