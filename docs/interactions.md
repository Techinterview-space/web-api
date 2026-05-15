# Interactions

External systems this service depends on at runtime, with the file that owns each integration. Configuration keys are the names from `src/Web.Api/appsettings.json`.

## Inbound

| From | Path | Notes |
|---|---|---|
| Frontend (browser) | `https://localhost:5001/api/...` | JWT bearer (or `[AllowAnonymous]` for public reads). CORS is fully open (`AllowAnyOrigin/Method/Header`) — see `Startup.ConfigureServices`. |
| Telegram Bot API | Long-poll (no inbound webhook) | Both bots pull updates over the outbound HTTP connection initiated by `AppInitializeService`. |
| Telegram (channel-stats) | `POST /api/integrations/telegram/...` (`Features/ChannelStats/Webhook/`) | Webhook for monitored channels. |
| SendGrid (legacy) | `POST` against the controllers in `Features/Webhooks/` modelled by `SendgridEventItem`. | Email-event webhook scaffolding. |
| Health probes | `GET /health` | Custom JSON response built by `HealthCheckCustomResponse.WriteAsync`. |
| Sitemap / RSS | `GET /sitemaps/...`, `GET /api/companies/reviews/recent.rss` | Anonymous. |
| Agent discovery | `GET /.well-known/oauth-protected-resource` | Anonymous. RFC 9728 metadata describing the API as a protected resource and pointing at the authorization server (`OAuth:Jwt:Issuer`). Owner: `Features/WellKnown/WellKnownController.cs`. |

## Outbound

### Persistence and search

- **PostgreSQL** — `ConnectionStrings:Database`. Owner: `src/Infrastructure/Database/DatabaseContext.cs`. Migrations in `src/Infrastructure/Migrations/` are applied at startup by `AppInitializeService.MigrateAsync`.
- **Elasticsearch** — `ConnectionStrings:Elasticsearch`. Owner: `src/Web.Api/Services/Logging/ElkSerilog.cs`. Used as a Serilog sink only; the API does not query it.

### Object storage

- **AWS S3 (LocalStack in Dev)** — `S3:*`. Owner: `Infrastructure/Services/Files/`. Two stores via `AspNetCore.Aws.S3.Simple`: `ICvStorage` (interview CV uploads) and `IPublicStorage` (public assets). Local Dev points at `http://localstack:4566/`.

### Email

- **Resend** — `ResendComApiKey`. Owner: `src/Infrastructure/Emails/ResendEmailApiSender.cs`. Used outside Development.
- **Local sender** — `src/Infrastructure/Emails/LocalEmailApiSender.cs`. Used in Development; logs the email payload instead of sending.
- **Templating** — `IViewRenderer` renders Razor view templates from `src/Web.Api/Views/`.

### Telegram

Two bots run inside the API process via `Coravel`-managed hosted services:

- **Salaries bot** — `SalariesTelegramBotHostedService` + `ISalariesTelegramBotClientProvider`. Handles salary stats, AI analyses, and subscription publishing.
- **Github profile bot** — `GithubProfileBotHostedService` + `IGithubProfileBotProvider`. Drives GitHub profile analysis chats.

Bot configuration (`TelegramBotConfiguration`) is read from the database, cached by `TelegramBotConfigurationService`. Tokens are not in `appsettings.json`.

### AI

- **OpenAI** — `OpenAI:ApiKey` / `BaseUrl` / `DefaultModel`. Owner: `Infrastructure/Ai/ChatGpt/`.
- **Claude (Anthropic)** — `Claude:ApiKey` / `BaseUrl` / `DefaultModel`. Owner: `Infrastructure/Ai/Claude/`.
- Provider selection: `IAiProviderFactory` (`Infrastructure/Ai/AiProviderFactory.cs`); `IArtificialIntellectService` (`Infrastructure/Services/AiServices/`) is the high-level entry point.

### GitHub

- **GitHub GraphQL** — owner: `Infrastructure/Services/Github/`. Uses the user's personal access token (`GithubPersonalUserToken`) or a development PAT from `Telegram:GithubPATForLocalDevelopment`. Optimisation notes in `docs/github-api-optimization.md`.

### Currencies

- **National Bank of Kazakhstan** — `Currencies:Url` (default `https://nationalbank.kz/rss/rates_all.xml`). Owner: `Infrastructure/Currencies/CurrenciesHttpService.cs`. Pulled daily by `RefetchServiceCurrenciesJob` and persisted as a `CurrenciesCollection`.

## Recurring jobs (Coravel)

Defined in `src/Web.Api/Setup/ScheduleConfig.cs`. All jobs use `PreventOverlapping`. When a debugger is attached, several also fire every minute for manual testing — that branch should never run in production.

| Job | Trigger |
|---|---|
| `RefetchServiceCurrenciesJob` | Daily 06:00; runs once at startup. |
| `SalariesHistoricalDataJob` | Daily 13:00. |
| `SalariesHistoricalDataBackfillJob` | Every 15 minutes. |
| `TelegramSalariesRegularStatsUpdateJob` | Daily 07:00; runs once at startup. |
| `SalariesSubscriptionPublishMessageJob` | Wednesdays 06:00. |
| `SalariesAiAnalysisSubscriptionWeeklyJob` | Wednesdays 05:00 (runs before the publish job). |
| `CompanyReviewsAiAnalysisSubscriptionWeeklyJob` | Fridays 06:00. |
| `ChannelStatsMonthlyAggregationJob` | Daily 15:00. |
| `SalaryUpdateReminderEmailJob` | Hourly staggered (10 schedules at `i:30`, `i = 0..9`). |

## Configuration surface

`src/Web.Api/appsettings.json` is the schema reference. Production values are written into `src/Web.Api/appsettings.Production.json` at build time by the `sed` substitutions in `.github/workflows/deploy.yml`. Anything you add to settings has to flow through both — see `docs/gotchas.md`.
