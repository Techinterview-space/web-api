# Changelog

## 2026-02-12

- Added a full public surveys domain model with `PublicSurvey`, `PublicSurveyQuestion`, `PublicSurveyOption`, `PublicSurveyResponse`, `PublicSurveyResponseOption`, and `PublicSurveyStatus`.
- Implemented survey lifecycle and domain validation rules, including draft/published/closed transitions, soft delete/restore, slug normalization, option count limits, and response invariants.
- Added EF Core persistence for surveys and responses in `DatabaseContext` and new entity configurations with key constraints and indexes (including unique non-deleted slug and one response per user per question).
- Added migration `20260212154526_PublicSurveyEntitiesAdded` that creates `PublicSurveys`, `PublicSurveyQuestions`, `PublicSurveyOptions`, `PublicSurveyResponses`, and `PublicSurveyResponseOptions`.
- Added full Public Surveys API feature set in `Web.Api` (handlers, requests/commands, DTOs, and controller routes) for create, update, publish, close, reopen, delete, restore, list-by-author, get-by-slug, submit-response, and results retrieval.
- Added public survey test fakes in `src/TestUtils/Fakes` and comprehensive coverage in `src/Domain.Tests/Entities/Surveys` and `src/Web.Api.Tests/Features/PublicSurveys`.
- Added `dotnet-skill.md` with .NET backend architecture and best-practices guidance.

## 2026.1.6

### Breaking Changes

- **Auth0 Removed** - Replaced Auth0 authentication provider with native JWT-based authentication system

### New Features

- **Native Authentication System**
  - Added `M2mClient` and `M2mClientScope` entities for machine-to-machine client management
  - Added `RefreshToken` entity for secure token refresh functionality
  - User entity now supports password-based authentication with:
    - Password hash storage
    - Password reset tokens with expiration
    - Email verification tokens with expiration
    - Failed login attempt tracking
    - Account lockout mechanism (5 failed attempts = 15 min lockout)
  - Added `RequiresScopeAttribute` for M2M authorization on endpoints
  - Added factory method `User.CreateFromExternalProviderAuth()` for OAuth provider flows

- **Environment Configuration**
  - Added `.env` file support for local development configuration

### Dependencies

- Added `BCrypt.Net-Next` (v4.0.3) package for secure password hashing
- Added `System.IdentityModel.Tokens.Jwt` (v8.15.0) to Domain project for JWT handling

### Database

- Added migration `20260126174501_Auth0RemovedFromSystem` with new authentication tables and user fields

### Code Quality

- Removed BOM (Byte Order Mark) from source files across the codebase

### CI/CD

- Updated GitHub Actions deploy workflow for new authentication configuration


