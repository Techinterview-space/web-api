# Tech.Interviewer Web API

![CodeRabbit Pull Request Reviews](https://img.shields.io/coderabbit/prs/github/Techinterview-space/web-api?utm_source=oss&utm_medium=github&utm_campaign=Techinterview-space%2Fweb-api&labelColor=171717&color=FF570A&link=https%3A%2F%2Fcoderabbit.ai&label=CodeRabbit+Reviews)

Backend API for [techinterview.space](https://techinterview.space) — interview management, salary data, company reviews, and AI-powered analysis. Monolithic ASP.NET Core service over PostgreSQL, with Elasticsearch, S3, Resend, Telegram, OpenAI, Claude, and the GitHub GraphQL API.

- Frontend: <https://techinterview.space>
- API (production): <https://api.techinterview.space>

## Prerequisites

- .NET 10 SDK ([download](https://dotnet.microsoft.com/en-us/download/dotnet/10.0))
- Docker Desktop (or any Docker Engine + `docker-compose`)
- An IDE that understands `.sln`: Rider, Visual Studio, or VS Code with the C# extension
- macOS, Linux, or Windows

## Run locally

```bash
# 1. Start dependencies (Postgres, Elasticsearch, LocalStack S3)
docker-compose up -d --build database.api elasticsearch localstack

# 2. Run the API
cd src
dotnet run --project Web.Api
```

URLs:

- API: `https://localhost:5001`
- Swagger UI: `https://localhost:5001/swagger`
- Health: `https://localhost:5001/health`

The first run will pull and apply EF Core migrations automatically (`AppInitializeService.MigrateAsync`).

## Build, lint, test

```bash
cd src
dotnet build         # also runs StyleCop and standard.ruleset as errors
dotnet test          # runs Domain.Tests, InfrastructureTests, Web.Api.Tests
```

CI: `.github/workflows/test.yml` builds and tests on every PR; `.github/workflows/deploy.yml` builds the Docker image and SCPs the deploy compose file on pushes to `main`.

## Documentation

- [`AGENTS.md`](./AGENTS.md) — agent rules and quick orientation
- [`CLAUDE.md`](./CLAUDE.md) — Claude Code working agreement
- [`docs/architecture.md`](./docs/architecture.md) — runtime topology, projects, mediator pattern, scheduler
- [`docs/authentication.md`](./docs/authentication.md) — OAuth, JWT, refresh tokens, M2M, role and scope filters
- [`docs/domain.md`](./docs/domain.md) — top-level aggregates and shared base types
- [`docs/interactions.md`](./docs/interactions.md) — external systems and recurring jobs
- [`docs/testing.md`](./docs/testing.md) — test infrastructure and fakes
- [`docs/gotchas.md`](./docs/gotchas.md) — non-obvious behaviour to know about
- [`docs/github-api-optimization.md`](./docs/github-api-optimization.md) — GitHub GraphQL usage notes
- [`CONTRIBUTING.md`](./CONTRIBUTING.md), [`CODE_OF_CONDUCT.md`](./CODE_OF_CONDUCT.md)

## RSS feeds

Approved company reviews are exposed as RSS 2.0:

```
GET /api/companies/reviews/recent.rss
```

Query parameters: `page` (default `1`), `pageSize` (default `50`, max `100`). Example: `https://api.techinterview.space/companies/reviews/recent.rss?pageSize=20`.
