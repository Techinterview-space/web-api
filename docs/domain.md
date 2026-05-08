# Domain

The `Domain` project (`src/Domain/`) is pure: no I/O, no EF Core, no ASP.NET. It defines entities, value objects, validation, and enums. The full set of aggregates lives under `src/Domain/Entities/`; this page describes the shared base types and the shape of each top-level area, and points at the source.

## Shared base types

- `BaseModel` — marker for entities with a `long` primary key and validation hooks.
- `IBaseModel` / `IHasId` / `IHasIdBase<TKey>` — id contracts.
- `HasDatesBase` and `IHasDates` — `CreatedAt` / `UpdatedAt` handled by `DatabaseContext.OnBeforeSaving`.
- `IHasDeletedAt` — soft-delete marker. There is **no** EF Core query filter; consumers must call `.Active()` from `Infrastructure.Database.ContextExtensions`. See `docs/gotchas.md`.

`Domain.Validation` and `Domain.Validation.Exceptions` define the exception hierarchy that `ExceptionHttpMiddleware` maps to HTTP responses (`BadRequestException`, `NotFoundException`, `NoPermissionsException`, `DatabaseException`, `UnauthorizedException`, etc.).

`Domain.ValueObjects` holds reusable value types: `CurrentUser` (claims wrapper), pagination (`PageModel`, `Pageable<T>`), slug helpers (`KebabCaseSlug`, `CompanySlug`), and date types under `Domain.ValueObjects.Dates`.

## Top-level aggregates

| Area | Source | Notes |
|---|---|---|
| Users and auth | `Domain/Entities/Users/`, `Domain/Entities/Auth/` | Native auth (password + OAuth identity), `RefreshToken`, M2M (`M2mClient`, `M2mClientScope`), `UserEmail`, `UserLabel`. Account lockout is on the `User` entity. |
| Interviews | `Domain/Entities/Interviews/` | `Interview`, `InterviewSubject`, `InterviewTemplate`, `InterviewTemplateSubject`, `ShareLink`. PDF export rendered by `IInterviewPdfService`. |
| Salaries | `Domain/Entities/Salaries/` | `UserSalary`, `Profession`, `Skill`, `WorkIndustry`, `Currency`, plus survey replies and the historical-data sub-tree. Soft-deletable. |
| Companies and reviews | `Domain/Entities/Companies/` | `Company`, `CompanyReview`, `CompanyReviewVote`, `CompanyRatingHistory`, `CompanyOpenAiAnalysis`. `Company` is soft-deletable; reviews carry their own moderation lifecycle and approval state. |
| Public surveys | `Domain/Entities/Surveys/`, `Domain/Entities/Questions/` | `PublicSurvey` lifecycle: draft → published → closed, plus soft delete and restore. Slug is unique among non-deleted surveys. One response per user per question. |
| Subscriptions | `Domain/Entities/StatData/` | `StatDataChangeSubscription` (salary alerts), `LastWeekCompanyReviewsSubscription`, `JobPostingMessageSubscription`. Drive the weekly Telegram and AI summaries. |
| AI analysis | `Domain/Entities/StatData/`, `Domain/Entities/Companies/`, `Domain/Entities/Prompts/` | `AiAnalysisRecord`, `CompanyOpenAiAnalysis`, `OpenAiPrompt`. |
| GitHub profiles | `Domain/Entities/Github/` | `GithubProfile`, processing job queue, bot chat history, personal access tokens. |
| Telegram | `Domain/Entities/Telegram/` | Bot configurations, user settings, raw update log, inline replies, salary bot messages, channel-stats domain (`MonitoredChannel`, `ChannelPost`, `MonthlyStatsRun`). |
| Currencies | `Domain/Entities/Currencies/` | `CurrenciesCollection` snapshots fetched from the National Bank of Kazakhstan XML feed. |
| CSV exports | `Domain/Entities/CSV/` | `UserCsvDownload` audit. |
| Historical records | `Domain/Entities/HistoricalRecords/` | Salary historical data templates and records. |

## Identity conventions

- Entities with a `Guid` primary key set `Id = Guid.NewGuid()` in their constructor and **must** be configured with `ValueGeneratedNever()` in their EF config to avoid the `Modified`-instead-of-`Added` bug. See `docs/gotchas.md`.
- Long-keyed entities use the EF default identity column.

## Where validation lives

Domain types validate themselves on construction and throw from `Domain.Validation.Exceptions`. `DatabaseContext.SaveAsync<TEntity>` and the public collection variant call `entity.ThrowIfInvalid()` before adding, so persisting an invalid entity fails fast.
