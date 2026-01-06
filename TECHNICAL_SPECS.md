# Technical Specifications - Tech.Interviewer Web API

## Table of Contents

1. [Project Overview](#project-overview)
2. [Architecture](#architecture)
3. [Solution Structure](#solution-structure)
4. [Technology Stack](#technology-stack)
5. [Domain Model](#domain-model)
6. [Database Architecture](#database-architecture)
7. [Services and Features](#services-and-features)
8. [Authentication & Authorization](#authentication--authorization)
9. [External Integrations](#external-integrations)
10. [API Design](#api-design)
11. [Background Jobs & Scheduling](#background-jobs--scheduling)
12. [Logging & Monitoring](#logging--monitoring)
13. [Code Standards & Patterns](#code-standards--patterns)
14. [Development Guide](#development-guide)
15. [Deployment](#deployment)

---

## Project Overview

Tech.Interviewer Web API is a monolithic backend API for [techinterview.space](https://techinterview.space), a platform for developer interview management, salary data collection and analysis, company reviews, and AI-powered insights.

**Repository:** https://github.com/techinterview-space/web-api

**Key Features:**
- Interview management and PDF generation
- Salary data collection, analysis, and visualization
- Company reviews and ratings with AI analysis
- Telegram bot integration for salary notifications
- GitHub profile analysis
- Currency conversion and historical data tracking
- AI-powered company analysis (ChatGPT, Claude)

---

## Architecture

### Architectural Pattern

The application follows a **Clean Architecture / Domain-Driven Design (DDD)** approach with clear separation of concerns:

```
┌─────────────────────────────────────────────────────────┐
│                   Web.Api Layer                        │
│  (Controllers, Features, Middlewares, Setup)            │
└─────────────────────┬───────────────────────────────────┘
                      │
                      ↓
┌─────────────────────────────────────────────────────────┐
│                Infrastructure Layer                    │
│  (Database, External Services, AI, Email, Storage)    │
└─────────────────────┬───────────────────────────────────┘
                      │
                      ↓
┌─────────────────────────────────────────────────────────┐
│                    Domain Layer                         │
│  (Entities, Value Objects, Validation, Enums)          │
└─────────────────────────────────────────────────────────┘
```

### Layer Responsibilities

#### 1. Web.Api Layer (Presentation)
- **Controllers**: API endpoints organized by feature
- **Features**: Request/Command handlers (CQRS-like pattern)
- **Middlewares**: Request/response processing
- **Setup**: Dependency injection configuration

#### 2. Infrastructure Layer (Application)
- Database context and EF Core configurations
- External service integrations (AI, Email, Storage)
- Business logic services
- Mediator pattern implementation

#### 3. Domain Layer (Core Business Logic)
- Entity models
- Value objects
- Business rules and validation
- Domain events

### Design Patterns

1. **CQRS (Command Query Responsibility Segregation)**
   - Separate handlers for commands and queries
   - `IRequestHandler<TRequest, TResult>` interface

2. **Repository Pattern**
   - EF Core `DbContext` as aggregate root
   - Custom extensions for complex queries

3. **Specification Pattern**
   - Fluent query builders for complex searches

4. **Mediator Pattern**
   - `IRequestHandler<TRequest, TResult>` for request/response handling

5. **Dependency Injection**
   - Constructor injection throughout
   - Scoped, Transient, Singleton lifetimes as appropriate

---

## Solution Structure

```
src/
├── Domain/                      # Core domain layer
│   ├── Attributes/             # Custom validation attributes
│   ├── Entities/               # Domain entities
│   │   ├── Companies/          # Company-related entities
│   │   ├── Interviews/         # Interview entities
│   │   ├── Salaries/           # Salary data entities
│   │   ├── Users/              # User entities
│   │   ├── Github/             # GitHub profile entities
│   │   ├── Telegram/           # Telegram bot entities
│   │   ├── StatData/           # Statistics and subscriptions
│   │   ├── Labels/             # Label entities
│   │   ├── Prompts/            # AI prompt entities
│   │   ├── CSV/                # CSV export entities
│   │   ├── Currencies/         # Currency entities
│   │   └── HistoricalRecords/  # Historical data entities
│   ├── Enums/                  # Domain enumerations
│   ├── Extensions/             # Domain extension methods
│   ├── Totp/                   # TOTP (Two-Factor Auth) logic
│   ├── Validation/             # Validation framework
│   ├── ValueObjects/           # Value objects
│   │   ├── Dates/             # Date-related value objects
│   │   ├── Pagination/        # Pagination logic
│   │   └── Ranges/            # Range value objects
│   └── Domain.csproj
│
├── Infrastructure/              # Infrastructure layer
│   ├── Ai/                    # AI service providers
│   │   ├── ChatGpt/           # OpenAI ChatGPT integration
│   │   └── Claude/            # Anthropic Claude integration
│   ├── Authentication/         # Authentication services
│   ├── Configs/               # Configuration classes
│   ├── Currencies/            # Currency services
│   ├── Database/              # EF Core database
│   │   ├── Config/           # Entity configurations
│   │   └── Extensions/       # Database extensions
│   ├── Emails/                # Email services
│   ├── Extensions/            # Infrastructure extensions
│   ├── Images/                # Image processing
│   ├── Jwt/                   # JWT token generation
│   ├── Migrations/            # Database migrations
│   ├── Salaries/              # Salary data services
│   ├── Services/              # Application services
│   │   ├── AiServices/        # AI-related services
│   │   ├── Companies/         # Company services
│   │   ├── Correlation/       # Correlation ID tracking
│   │   ├── Files/             # File storage (S3)
│   │   ├── Github/            # GitHub API services
│   │   ├── Html/              # HTML/Markdown generation
│   │   ├── Mediator/          # Mediator pattern
│   │   ├── PDF/               # PDF generation
│   │   ├── Professions/       # Professions caching
│   │   └── Telegram/          # Telegram bot services
│   └── Infrastructure.csproj
│
├── Web.Api/                   # Presentation layer
│   ├── Features/              # Feature modules (CQRS handlers)
│   │   ├── Accounts/         # Account management
│   │   ├── Admin/            # Admin features
│   │   ├── Companies/        # Company endpoints
│   │   ├── CompanyReviews/   # Company reviews
│   │   ├── Emails/           # Email features
│   │   ├── Github/           # GitHub features
│   │   ├── Interviews/       # Interview endpoints
│   │   ├── Salaries/         # Salary endpoints
│   │   ├── Surveys/          # Survey features
│   │   ├── Telegram/         # Telegram features
│   │   └── Users/            # User endpoints
│   ├── Middlewares/           # Custom middlewares
│   ├── Services/              # Web.Api specific services
│   ├── Setup/                 # DI and configuration
│   ├── Views/                 # Razor views (emails)
│   ├── Program.cs             # Application entry point
│   ├── Startup.cs             # Startup configuration
│   └── Web.Api.csproj
│
├── TestUtils/                 # Testing utilities
│   ├── Auth/                  # Test auth utilities
│   ├── Db/                    # In-memory database utilities
│   ├── Fakes/                 # Entity factories
│   ├── Mocks/                 # Service mocks
│   └── TestUtils.csproj
│
├── Domain.Tests/               # Domain layer tests
├── InfrastructureTests/        # Infrastructure layer tests
└── Web.Api.Tests/            # Presentation layer tests
```

---

## Technology Stack

### Core Framework
- **.NET 10** (Target Framework: `net10.0`)
- **C# Latest** (Language version: latest)

### Database
- **PostgreSQL** 13 (via Docker)
- **Entity Framework Core** 10.0.1
- **Npgsql.EntityFrameworkCore.PostgreSQL** 10.0.0

### Web Server
- **ASP.NET Core** 10.0.0
- **Kestrel** (default web server)

### Infrastructure
- **Docker & Docker Compose** (containerization)
- **Elasticsearch** 8.0.0 (logging)
- **Kibana** 8.0.0 (log visualization)
- **LocalStack** (AWS services emulation)

### Key Libraries

| Category | Library | Version | Purpose |
|----------|---------|---------|---------|
| Authentication | Microsoft.AspNetCore.Authentication.JwtBearer | 10.0.0 | JWT authentication |
| AI Integration | GraphQL.Client | 6.1.0 | GitHub GraphQL API |
| PDF Generation | QuestPDF | 2025.12.0 | PDF generation |
| Email | SendGrid | 9.29.3 | Email delivery |
| Email | StrongGrid | 0.114.0 | Advanced SendGrid features |
| Telegram | Telegram.Bot | 22.7.6 | Telegram bot |
| Storage | AspNetCore.Aws.S3.Simple | 0.1.5 | AWS S3 integration |
| CSV | CsvHelper | 33.1.0 | CSV parsing |
| TOTP | Otp.NET | 1.4.1 | Two-factor authentication |
| Environment | dotenv.net | 4.0.0 | .env file loading |
| Logging | Serilog | 3.1.1 | Structured logging |
| Elasticsearch Sink | Serilog.Sinks.Elasticsearch | 9.0.3 | Elasticsearch logging |
| Scheduling | Coravel | 6.0.2 | Task scheduling |
| Swagger | Swashbuckle.AspNetCore | 10.1.0 | API documentation |

### AI Providers
- **ChatGPT** (OpenAI) - AI analysis and chat
- **Claude** (Anthropic) - Alternative AI provider

---

## Domain Model

### Core Entities

#### 1. User (src/Domain/Entities/Users/User.cs)
Represents application users with multi-factor authentication support.

**Properties:**
- `Id` (long): Primary key
- `Email` (string): User email (unique, max 150 chars)
- `FirstName`, `LastName` (string): User names
- `IdentityId` (string): External auth provider ID (Google/GitHub/Auth0)
- `ProfilePicture` (string): Profile picture URL
- `EmailConfirmed` (bool): Email verification status
- `UniqueToken` (string): Unsubscribe token
- `TotpSecret` (string): TOTP secret key for 2FA
- `TotpVerifiedAt` (DateTimeOffset?): Last TOTP verification time
- `DeletedAt` (DateTimeOffset?): Soft delete timestamp
- `LastLoginAt` (DateTimeOffset?): Last login timestamp
- `UserRoles` (List<UserRole>): User roles
- `Salaries` (List<UserSalary>): User's salary records
- `Reviews` (List<CompanyReview>): User's company reviews
- `Votes` (List<CompanyReviewVote>): Review votes
- `Emails` (List<UserEmail>): User email history

**Key Methods:**
- `HasOrFail(Role)`: Authorization check
- `HasAnyOrFail(Role[])`: Multiple role check
- `VerifyTotp(string)`: Verify 2FA code
- `IsMfaEnabled()`: Check if 2FA is enabled
- `GenerateTotpSecretKey()`: Generate TOTP secret
- `ConfirmEmail()`: Mark email as confirmed
- `Delete()`/`Restore()`: Soft delete/restore

#### 2. Interview (src/Domain/Entities/Interviews/Interview.cs)
Represents developer interviews.

**Properties:**
- `Id` (Guid): Primary key (GUID-based)
- `CandidateName` (string): Interview candidate name
- `InterviewerId` (long): Foreign key to User
- `Interviewer` (User): Interviewer entity
- `OverallOpinion` (string): Overall interview opinion (max 50,000 chars)
- `CandidateGrade` (DeveloperGrade?): Candidate grade
- `ShareLink` (ShareLink): Shared link entity
- `Subjects` (List<InterviewSubject>): Interview subjects

#### 3. Company (src/Domain/Entities/Companies/Company.cs)
Represents companies with review and rating system.

**Properties:**
- `Id` (Guid): Primary key
- `Name` (string): Company name
- `NormalizedName` (string): Uppercase name for searching
- `Description` (string): Company description
- `Links` (List<string>): Company links
- `LogoUrl` (string): Logo URL
- `Rating` (double): Average rating (0-5)
- `ReviewsCount` (int): Number of reviews
- `ViewsCount` (int): View counter
- `Slug` (string): URL slug
- `DeletedAt` (DateTime?): Soft delete timestamp
- `RatingHistory` (List<CompanyRatingHistory>): Rating history
- `Reviews` (List<CompanyReview>): Company reviews
- `OpenAiAnalysisRecords` (List<CompanyOpenAiAnalysis>): AI analysis

**Key Methods:**
- `AddReview(CompanyReview)`: Add a review
- `ApproveReview(Guid)`: Approve and recalculate rating
- `RecalculateRating()`: Update average rating
- `MarkReviewAsOutdated(Guid)`: Mark review as outdated

#### 4. UserSalary (src/Domain/Entities/Salaries/UserSalary.cs)
Represents salary data submissions.

**Properties:**
- `Id` (Guid): Primary key
- `UserId` (long?): User ID
- `User` (User): User entity
- `Value` (double): Salary amount
- `Quarter` (int): Quarter (1-4)
- `Year` (int): Year (2000-3000)
- `Currency` (Currency): Currency type
- `Grade` (DeveloperGrade?): Developer grade
- `Company` (CompanyType): Company type
- `ProfessionEnum` (UserProfessionEnum): Profession enum
- `City` (KazakhstanCity?): City
- `Age` (int?): Age
- `YearOfStartingWork` (int?): Work start year
- `Gender` (Gender?): Gender
- `UseInStats` (bool): Include in statistics
- `SkillId` (long?): Skill ID
- `WorkIndustryId` (long?): Work industry ID
- `ProfessionId` (long?): Profession ID
- `SourceType` (SalarySourceType?): Source type

#### 5. CompanyReview (src/Domain/Entities/Companies/CompanyReview.cs)
Company review with rating system.

**Properties:**
- `Id` (Guid): Primary key
- `CompanyId` (Guid): Company ID
- `Company` (Company): Company entity
- `UserId` (long): User ID
- `User` (User): User entity
- `Title` (string): Review title
- `Body` (string): Review body
- `Ratings` (Dictionary<string, int>): Rating categories
- `TotalRating` (double): Overall rating
- `ApprovedAt` (DateTimeOffset?): Approval timestamp
- `OutdatedAt` (DateTimeOffset?): Outdated timestamp

### Enums

#### Role (src/Domain/Enums/Role.cs)
```csharp
Undefined = 0,
Interviewer = 1,
Admin = 1024
```

#### DeveloperGrade (src/Domain/Entities/Enums/DeveloperGrade.cs)
Standard developer grade levels (Junior, Middle, Senior, Tech Lead, etc.)

#### Gender (src/Domain/Enums/Gender.cs)
```csharp
Undefined = 0,
Female = 1,
Male = 2,
PreferNotToSay = 3,
Other = 4
```

#### KazakhstanCity (src/Domain/Enums/KazakhstanCity.cs)
Kazakhstan cities for location-based filtering

#### CompanyType (src/Domain/Entities/Salaries/CompanyType.cs)
Company types (Product, Outsource, Startup, etc.)

#### UserProfessionEnum (src/Domain/Entities/Salaries/UserProfessionEnum.cs)
Developer professions (Backend, Frontend, Fullstack, DevOps, etc.)

### Value Objects

#### CurrentUser (src/Domain/ValueObjects/CurrentUser.cs)
Represents the authenticated user from HTTP context.

**Properties:**
- `UserId` (string): Auth provider user ID
- `Email` (string): User email
- `FirstName`, `LastName` (string): User names
- `ProfilePicture` (string): Profile picture URL
- `Roles` (List<Role>): User roles

**Methods:**
- `IsGoogleAuth()`: Check if Google OAuth
- `IsGithubAuth()`: Check if GitHub OAuth
- `IsAuth0Auth()`: Check if Auth0

#### CompanySlug (src/Domain/ValueObjects/CompanySlug.cs)
Company slug generation and validation.

#### KebabCaseSlug (src/Domain/ValueObjects/KebabCaseSlug.cs)
Kebab-case slug generation.

#### PageModel (src/Domain/ValueObjects/Pagination/PageModel.cs)
Pagination parameters (page, pageSize).

#### PercentileCollection (src/Domain/ValueObjects/PercentileCollection.cs)
Statistical percentile calculations.

---

## Database Architecture

### Database Context
**Location:** `src/Infrastructure/Database/DatabaseContext.cs`

**DbContext:** `DatabaseContext : DbContext`

### DbSets (Tables)

| Entity | DbSet Name | Primary Key Type |
|--------|------------|------------------|
| User | Users | long |
| InterviewTemplate | InterviewTemplates | long |
| Interview | Interviews | Guid |
| ShareLink | ShareLinks | long |
| UserLabel | UserLabels | long |
| UserSalary | Salaries | Guid |
| Skill | Skills | long |
| WorkIndustry | WorkIndustries | long |
| Profession | Professions | long |
| SalariesBotMessage | SalariesBotMessages | long |
| TelegramUserSettings | TelegramUserSettings | long |
| UserCsvDownload | UserCsvDownloads | long |
| SalariesSurveyReply | SalariesSurveyReplies | long |
| StatDataChangeSubscription | SalariesSubscriptions | long |
| Company | Companies | Guid |
| CompanyRatingHistory | CompanyRatingHistoryRecords | long |
| CompanyReview | CompanyReviews | Guid |
| CompanyReviewVote | CompanyReviewVotes | long |
| AiAnalysisRecord | AiAnalysisSubscriptionRecords | long |
| TelegramInlineReply | TelegramInlineReplies | long |
| SubscriptionTelegramMessage | SalariesSubscriptionTelegramMessages | long |
| UserEmail | UserEmails | long |
| GithubProfile | GithubProfiles | long |
| GithubProfileBotChat | GithubProfileBotChats | long |
| GithubProfileBotMessage | GithubProfileBotMessages | long |
| GithubPersonalUserToken | GithubPersonalUserTokens | long |
| GithubProfileProcessingJob | GithubProfileProcessingJobs | long |
| OpenAiPrompt | OpenAiPrompts | long |
| CompanyOpenAiAnalysis | CompanyOpenAiAnalysisRecords | long |
| LastWeekCompanyReviewsSubscription | CompanyReviewsSubscriptions | long |
| JobPostingMessageSubscription | JobPostingMessageSubscriptions | long |
| SalariesHistoricalDataRecordTemplate | SalariesHistoricalDataRecordTemplates | long |
| SalariesHistoricalDataRecord | SalariesHistoricalDataRecords | long |
| CurrenciesCollection | CurrencyCollections | long |

### Database Configuration

**Location:** `src/Infrastructure/Database/Config/`

Entity configurations use Fluent API:
- Indexes (unique, composite)
- Relationships (one-to-many, many-to-many)
- Property constraints (max length, required, etc.)
- Query filters (soft delete)

**Key Configurations:**
- `UserDbConfig.cs`: User entity indexes (Email, IdentityId, UniqueToken)
- `CompanyEntityConfiguration.cs`: Company entity configuration
- `InterviewConfig.cs`: Interview entity configuration
- `UserSalaryConfig.cs`: Salary entity configuration

### Migrations

**Location:** `src/Infrastructure/Migrations/`

⚠️ **IMPORTANT:** Migrations are manually generated and should NOT be modified directly.

**Migration Generation:**
```powershell
./add-migration.ps1 <MigrationName>
```

Or using EF CLI:
```bash
dotnet ef migrations add <MigrationName> --project Infrastructure --startup-project Web.Api
```

### Database Methods

**Save Operations:**
```csharp
await context.SaveAsync<TEntity>(entity)
await context.SaveAsync(entities)
await context.RemoveAsync(entity)
```

**Transaction Support:**
```csharp
await context.DoWithinTransactionAsync<TResult>(
    action,
    errorMessage,
    cancellationToken)
```

---

## Services and Features

### Feature Pattern (CQRS)

All features follow the CQRS pattern:
- **Commands**: Write operations
- **Queries**: Read operations
- **Handlers**: `IRequestHandler<TRequest, TResult>` implementation

**Example Handler Structure:**
```csharp
public class FeatureHandler : IRequestHandler<FeatureRequest, FeatureResponse>
{
    public async Task<FeatureResponse> Handle(
        FeatureRequest request,
        CancellationToken cancellationToken)
    {
        // Implementation
    }
}
```

### Key Features

#### 1. Accounts (src/Web.Api/Features/Accounts/)
- Account management
- TOTP (2FA) setup and verification
- User profile management

**Controllers:**
- `AccountController.cs`
- `TotpController.cs`

#### 2. Interviews (src/Web.Api/Features/Interviews/)
- Interview creation and management
- Interview templates
- PDF generation
- Share links

**Controllers:**
- `InterviewsController.cs`
- `InterviewTemplateController.cs`

**Services:**
- `IInterviewPdfService` - PDF generation using QuestPDF

#### 3. Companies (src/Web.Api/Features/Companies/)
- Company CRUD operations
- Company search
- Company details

**Controllers:**
- `CompaniesController.cs`

#### 4. Company Reviews (src/Web.Api/Features/CompanyReviews/)
- Add company reviews
- Approve/reject reviews
- Vote on reviews (like/dislike)
- RSS feed for recent reviews
- Search reviews

**Controllers:**
- `CompanyReviewsController.cs`

**RSS Endpoint:**
```
GET /api/companies/reviews/recent.rss
```

#### 5. Salaries (src/Web.Api/Features/Salaries/)
- Salary data submission
- Salary statistics and charts
- Salary filtering and analysis
- Historical data

**Controllers:**
- `SalariesController.cs`
- `SalariesAdminController.cs`
- `SalariesChartShare/ChartShareController.cs`

**Features:**
- Salary charts (by grade, experience, city, gender, etc.)
- Percentile calculations
- Currency conversion
- Historical data templates

#### 6. Salary Subscriptions (src/Web.Api/Features/SalarySubscribtions/)
- Telegram subscriptions for salary updates
- Subscription management (activate/deactivate)
- AI-powered salary reports

**Controllers:**
- `SalaryTelegramSubscriptionsController.cs`

**Services:**
- `SalariesSubscriptionService`

#### 7. GitHub Integration (src/Web.Api/Features/Github/)
- GitHub profile analysis
- GitHub personal tokens
- Profile processing jobs

**Controllers:**
- `GithubController.cs`

**Services:**
- `IGithubGraphQLService` - GitHub API integration
- `IGithubPersonalUserTokenService` - Token management

#### 8. Telegram Bots (src/Web.Api/Features/Telegram/)
- Salary notification bot
- GitHub profile bot
- Inline replies
- User settings

**Controllers:**
- `TelegramBotController.cs`

**Hosted Services:**
- `SalariesTelegramBotHostedService`
- `GithubProfileBotHostedService`

**Services:**
- `ISalariesTelegramBotClientProvider`
- `IGithubProfileBotProvider`
- `ITelegramAdminNotificationService`

#### 9. Admin Features (src/Web.Api/Features/Admin/)
- Admin dashboard
- Debug tools
- AI prompt management
- System administration

**Controllers:**
- `AdminDashboardController.cs`
- `AdminToolsController.cs`
- `DebugController.cs`
- `AiPromptController.cs`

#### 10. Dictionaries (src/Web.Api/Features/Dictionaries/)
- Skills management
- Professions management
- Work industries management

**Controllers:**
- `SkillsController.cs`
- `ProfessionsController.cs`
- `WorkIndustriesController.cs`

**Services:**
- `IProfessionsCacheService` - Cached professions data

#### 11. Surveys (src/Web.Api/Features/Surveys/)
- Salary surveys
- Survey statistics
- Survey responses

**Controllers:**
- `SurveyController.cs`

#### 12. Import/Export (src/Web.Api/Features/Import/)
- CSV data export
- Data import utilities

**Controllers:**
- `ImportController.cs`

#### 13. Webhooks (src/Web.Api/Features/Webhooks/)
- External webhook handling

**Controllers:**
- `WebhooksController.cs`

---

## Authentication & Authorization

### Authentication Flow

The application supports multiple authentication providers:
1. **Google OAuth2**
2. **GitHub OAuth**
3. **Auth0**

### Authentication Service

**Location:** `src/Infrastructure/Authentication/AuthorizationService.cs`

**Key Methods:**
```csharp
Task<User> GetCurrentUserOrFailAsync()
Task<User> GetCurrentUserOrNullAsync()
Task HasRoleOrFailAsync(Role role)
Task HasAnyRoleOrFailAsync(Role[] roles)
```

### Current User Detection

Based on `IdentityId` prefix:
- **Google**: `google-oauth2|` prefix
- **GitHub**: `github|` prefix
- **Auth0**: `auth0|` prefix

### Authorization

**Attribute-based Authorization:**
```csharp
[HasAnyRole]          // Require any authenticated user
[HasAnyRole(Role.Admin)]  // Require specific role
```

**Location:** `src/Web.Api/Setup/Attributes/HasAnyRoleAttribute.cs`

### Role-Based Access Control (RBAC)

**Roles:**
- `Undefined = 0` - No role
- `Interviewer = 1` - Standard interviewer role
- `Admin = 1024` - Administrator role

### Two-Factor Authentication (2FA)

Using TOTP (Time-based One-Time Password):
- Library: **Otp.NET** 1.4.1
- Implementation: `src/Domain/Totp/`

**Key Methods:**
- `GenerateTotpSecretKey()`: Generate TOTP secret
- `VerifyTotp(string code)`: Verify 6-digit code
- `TotpVerificationExpired()`: Check if verification expired (6-hour window)

### JWT Token Generation

**Location:** `src/Infrastructure/Jwt/TechinterviewJwtTokenGenerator.cs`

---

## External Integrations

### 1. AI Services

#### OpenAI ChatGPT
**Location:** `src/Infrastructure/Ai/ChatGpt/`

**Interface:** `IAiProvider`
```csharp
Task<AiChatResult> AnalyzeChatAsync(
    string input,
    string systemPrompt,
    string model = null,
    string correlationId = null,
    CancellationToken cancellationToken)
```

**Features:**
- Company review analysis
- Salary data analysis
- General AI chat

#### Anthropic Claude
**Location:** `src/Infrastructure/Ai/Claude/`

**Features:**
- Alternative AI provider
- Same interface as ChatGPT

**Factory:** `IAiProviderFactory` selects provider based on configuration

### 2. Email Services

**Providers:**
- **SendGrid** (Production)
- **Local** (Development)

**Location:** `src/Infrastructure/Emails/`

**Interfaces:**
```csharp
IEmailApiSender
ITechinterviewEmailService
IViewRenderer (Razor views for email templates)
```

**Services:**
- `SendgridEmailApiSender`
- `LocalEmailApiSender`
- `TechInterviewerEmailService`

**Email Templates:** `src/Web.Api/Views/EmailTemplates/`

### 3. Storage Services

**AWS S3 Integration**
**Library:** `AspNetCore.Aws.S3.Simple` 0.1.5
**Location:** `src/Infrastructure/Services/Files/`

**Services:**
```csharp
IPublicStorage    // Public file storage
ICvStorage        // CV storage
```

**Implementations:**
- `PublicStorage` - S3 public storage
- `CvStorageS3Service` - S3 CV storage

### 4. Telegram Bots

**Library:** `Telegram.Bot` 22.7.6

**Services:**
- `SalariesTelegramBotClientProvider` - Salary notifications
- `GithubProfileBotProvider` - GitHub profile bot

**Features:**
- Salary update notifications
- Chart sharing via Telegram
- Inline replies
- User settings management

### 5. GitHub Integration

**Library:** `GraphQL.Client` 6.1.0

**Services:**
```csharp
IGithubGraphQLService         - GraphQL API
IGithubPersonalUserTokenService - Personal token management
```

**Features:**
- Profile data retrieval
- Repository analysis
- Bot interactions

### 6. Currency Services

**Location:** `src/Infrastructure/Currencies/`

**Interfaces:**
```csharp
ICurrencyService       - Currency operations
ICurrenciesHttpService - HTTP-based currency data
```

**Features:**
- Currency conversion
- Exchange rate caching
- Historical currency data

### 7. PDF Generation

**Library:** `QuestPDF` 2025.12.0

**Services:**
```csharp
IPdf                    - Base PDF service
IInterviewPdfService    - Interview-specific PDF generation
```

**Features:**
- Interview reports
- Markdown to PDF conversion
- Custom layouts

---

## API Design

### RESTful API Design

**Base URL:** `https://api.techinterview.space`

### Routing Conventions

```
/api/{resource}
/api/{resource}/{id}
/api/{resource}/{id}/{sub-resource}
```

**Examples:**
- `/api/companies` - List companies
- `/api/companies/{id}` - Get company details
- `/api/companies/{id}/reviews` - Get company reviews

### Controller Pattern

**Feature-based controllers:**
```csharp
[ApiController]
[Route("api/companies")]
public class CompaniesController : ControllerBase
{
    // Actions
}
```

### Request/Response Pattern

**Commands (Write):**
```csharp
public class CreateCompanyCommand { }
public class CreateCompanyHandler : IRequestHandler<CreateCompanyCommand, Guid> { }
```

**Queries (Read):**
```csharp
public class GetCompanyQuery { }
public class GetCompanyHandler : IRequestHandler<GetCompanyQuery, CompanyDto> { }
```

### Mediator Pattern

**Usage in Controllers:**
```csharp
return Ok(await _serviceProvider.HandleBy<Handler, Request, Response>(
    request,
    cancellationToken));
```

**Location:** `src/Infrastructure/Services/Mediator/ServiceProviderExtensions.cs`

### Pagination

**Model:** `PageModel` (src/Domain/ValueObjects/Pagination/PageModel.cs)

**Properties:**
- `Page` (int): Page number
- `PageSize` (int): Items per page

**Response:** `Pageable<T>` (src/Domain/ValueObjects/Pagination/PaginatedListBase.cs)

### Validation

**Request Validation:**
- Fluent validation through data annotations
- `EntityValidator<T>` for entity validation
- `ThrowIfInvalid()` method

**Custom Attributes:**
- `NotDefaultValueAttribute` - Validate not default value
- `CollectionNotEmptyBaseAttribute` - Validate non-empty collections

### Error Handling

**Middleware:** `ExceptionHttpMiddleware`

**Exception Types:**
- `NotFoundException` - 404
- `BadRequestException` - 400
- `NoPermissionsException` - 403
- `EntityInvalidException` - 400
- `DatabaseException` - 500

**Location:** `src/Domain/Validation/Exceptions/`

### Correlation ID

**Header:** `X-Correlation-Id`

**Middleware:** `CorrelationIdMiddleware`

**Service:** `ICorrelationIdAccessor`

Every request/response includes a correlation ID for tracing.

---

## Background Jobs & Scheduling

### Scheduling

**Library:** `Coravel` 6.0.2

**Configuration:** `src/Web.Api/Setup/ScheduleConfig.cs`

### Hosted Services

**Base Class:** `BackgroundServiceBase`
**Location:** `src/Web.Api/Setup/HostedServices/`

### Telegram Bot Hosted Services

1. **SalariesTelegramBotHostedService**
   - Processes salary update notifications
   - Handles bot commands
   - Sends scheduled messages

2. **GithubProfileBotHostedService**
   - Processes GitHub profile requests
   - Handles profile analysis
   - Sends results via Telegram

### Initialization Service

**AppInitializeService**
- Runs on application startup
- Initializes required data
- Validates configuration

---

## Logging & Monitoring

### Logging Framework

**Library:** `Serilog` 3.1.1

**Sinks:**
- Console (development)
- Elasticsearch (production)

### Configuration

**Location:** `src/Infrastructure/Services/Logging/ElkSerilog.cs`

**Setup in Startup:**
```csharp
new ElkSerilog(
    config: _configuration,
    appName: "TechInterview.API",
    connectionString: _configuration.GetConnectionString("Elasticsearch"),
    environmentName: _environment.EnvironmentName).Setup();
```

### Structured Logging

**Correlation ID:** Added to all log entries via `CorrelationIdEnricher`

**Log Levels:**
- Debug (development)
- Information (general)
- Warning (issues)
- Error (exceptions)
- Fatal (critical failures)

### Health Checks

**Location:** `src/Web.Api/Setup/Healthcheck/`

**Endpoints:**
- `/health` - Basic health check
- `/health/db` - Database health check

**Custom Response:** `HealthCheckCustomResponse.Setup(app)`

---

## Code Standards & Patterns

### Coding Standards

**Ruleset:** `src/standard.ruleset`

**StyleCop Analyzers:**
- SA (StyleCop) rules for code style
- CA (Code Analysis) rules for code quality

**Key Rules:**
- PascalCase for public members
- camelCase for private fields (with underscore prefix)
- Proper spacing and formatting
- XML documentation comments (optional)

### Naming Conventions

| Element | Style | Example |
|---------|-------|---------|
| Namespaces | PascalCase | `TechInterviewer.Features.Interviews` |
| Classes | PascalCase | `InterviewService` |
| Interfaces | PascalCase with "I" prefix | `IInterviewRepository` |
| Methods | PascalCase | `GetInterviewByIdAsync` |
| Properties | PascalCase | `CreatedAt` |
| Private fields | camelCase with underscore | `_dbContext` |
| Local variables | camelCase | `interviewId` |
| Parameters | camelCase | `cancellationToken` |
| Async methods | PascalCase with "Async" suffix | `CreateInterviewAsync` |

### File Organization

**Feature-based structure:**
```
Features/
├── FeatureName/
│   ├── FeatureNameController.cs
│   ├── CreateFeature/
│   │   ├── CreateFeatureCommand.cs
│   │   ├── CreateFeatureHandler.cs
│   │   └── CreateFeatureResponse.cs
│   └── GetFeature/
│       ├── GetFeatureQuery.cs
│       ├── GetFeatureHandler.cs
│       └── GetFeatureResponse.cs
```

### Validation Pattern

**Entity Validation:**
```csharp
entity.ThrowIfInvalid();
```

**Request Validation:**
```csharp
request.ThrowIfInvalid();
```

### Null Checking

**Extension Methods:** `src/Domain/Extensions/`
```csharp
entity.ThrowIfNull(nameof(entity));
collection.ThrowIfNullOrEmpty(nameof(collection));
```

### Async/Await

All async operations use `async`/`await` pattern:
```csharp
public async Task<Result> MethodAsync(CancellationToken cancellationToken)
{
    var result = await _service.DoSomethingAsync(cancellationToken);
    return result;
}
```

### Transaction Management

```csharp
await _context.DoWithinTransactionAsync<TResult>(
    async () =>
    {
        // Operations
        return result;
    },
    errorMessage,
    cancellationToken);
```

---

## Development Guide

### Prerequisites

1. **Docker Desktop** - Required for running infrastructure
2. **.NET 10 SDK** - Development framework
3. **IDE** - Visual Studio, Rider, or VS Code
4. **PostgreSQL Client** - For database management (optional)

### Getting Started

1. **Clone Repository:**
```bash
git clone https://github.com/techinterview-space/web-api.git
cd web-api
```

2. **Restore Dependencies:**
```bash
cd src
dotnet restore
```

3. **Start Infrastructure:**
```bash
cd ..
docker-compose up -d --build database.api elasticsearch localstack
```

4. **Configure Environment:**
```bash
cp .env.example .env
# Edit .env with your settings
```

5. **Run Application:**
```bash
cd src
dotnet run
```

**Application URL:** `https://localhost:5001`

### Development Tools

**Swagger UI:** `https://localhost:5001/swagger`

**Kibana:** `http://localhost:5601` (Log visualization)

**PostgreSQL:** `localhost:5432`

### Building

```bash
cd src
dotnet build
```

### Testing

**Run All Tests:**
```bash
dotnet test
```

**Run Specific Project:**
```bash
dotnet test Domain.Tests
dotnet test InfrastructureTests
dotnet test Web.Api.Tests
```

### Database Migrations

**Create Migration:**
```powershell
./add-migration.ps1 <MigrationName>
```

**Or using EF CLI:**
```bash
dotnet ef migrations add <MigrationName> \
  --project Infrastructure \
  --startup-project Web.Api
```

⚠️ **IMPORTANT:** Do not modify migration files or DbContext snapshots manually.

### Code Quality

**Run Build (includes analysis):**
```bash
dotnet build
```

**Check Ruleset:** Follow rules in `src/standard.ruleset`

### Debugging

**Development Mode:**
- `ASPNETCORE_ENVIRONMENT=Development`
- Detailed error pages
- PII logging enabled
- Swagger UI enabled

---

## Deployment

### Docker Deployment

**Docker Compose:** `docker-compose.yml`

**Services:**
- `database.api` - PostgreSQL 13
- `elasticsearch` - Elasticsearch 8.0.0
- `kibana` - Kibana 8.0.0
- `localstack` - AWS services emulation
- `api` - Web API application

### Deployment Compose

**File:** `docker-compose.deploy.yml`

**Production Deployment:**
```bash
docker-compose -f docker-compose.deploy.yml up -d --build
```

### Environment Variables

**Required Variables:**
- `ASPNETCORE_ENVIRONMENT`
- `ConnectionStrings__Database`
- `ConnectionStrings__Elasticsearch`
- `ASPNETCORE_Kestrel__Certificates__Default__Path`
- `ASPNETCORE_Kestrel__Certificates__Default__Password`

### Kubernetes Deployment

**Manifests:** `manifests/`

**Files:**
- `deployment.yml` - API deployment
- `service.yml` - Service definition
- `ingress.yml` - Ingress configuration
- `cert.yml` - Certificate configuration

**Stage Manifests:**
- `deployment-stage.yml`
- `service-stage.yml`

### CI/CD

**GitHub Actions:** `.github/workflows/`

**Workflows:**
- `deploy.yml` - Deployment workflow
- `test.yml` - Testing workflow

### Health Checks

**Endpoints:**
- `/health` - Application health
- `/health/db` - Database health

### Monitoring

**Elasticsearch & Kibana:**
- Structured logging
- Log aggregation
- Visualization dashboards

---

## Appendix

### Key File Locations

| File/Component | Location |
|----------------|----------|
| Startup Configuration | `src/Web.Api/Startup.cs` |
| Program Entry Point | `src/Web.Api/Program.cs` |
| Database Context | `src/Infrastructure/Database/DatabaseContext.cs` |
| Entity Configurations | `src/Infrastructure/Database/Config/` |
| Service Registration | `src/Web.Api/Setup/ServiceRegistration.cs` |
| Ruleset | `src/standard.ruleset` |
| Docker Compose | `docker-compose.yml` |
| Environment Example | `.env.example` |

### Configuration Files

| File | Purpose |
|------|---------|
| `appsettings.json` | Base configuration |
| `appsettings.Development.json` | Development settings |
| `appsettings.Production.json` | Production settings |
| `.env` | Environment variables (not committed) |

### Important Notes

1. **No Microservices:** This is a monolithic application
2. **Migration Restrictions:** Do not generate or modify migrations manually
3. **Code Style:** Always follow `standard.ruleset`
4. **Build Verification:** Run `dotnet build` after all changes
5. **Testing:** Use xUnit framework
6. **Test Utilities:** Leverage existing test infrastructure in `TestUtils/`

### External Documentation

- **Contribution Guidelines:** `CONTRIBUTING.md`
- **Code of Conduct:** `CODE_OF_CONDUCT.md`
- **AI Agent Guidelines:** `CLAUDE.md`
- **GitHub API Optimization:** `docs/github-api-optimization.md`

---

## Contact & Support

- **Repository:** https://github.com/techinterview-space/web-api
- **Issues:** https://github.com/techinterview-space/web-api/issues
- **Website:** https://techinterview.space
- **API:** https://api.techinterview.space

---

*Document Version: 1.0*
*Last Updated: January 2026*
*Framework: .NET 10*
