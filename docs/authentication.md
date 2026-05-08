# Authentication and authorization

The service issues its own JWTs. OAuth providers (Google, GitHub) are used only as identity sources; the access token returned to the frontend is always minted internally.

## Token model

Two parallel JWT flows live side-by-side:

- **User JWT** — issued after OAuth callback or a password login. Bearer scheme, signed with HMAC using `OAuth:Jwt:Secret` (`src/Infrastructure/Jwt/JwtTokenService.cs`). Default lifetime `OAuth:Jwt:AccessTokenExpirationMinutes` (60 in `appsettings.json`).
- **M2M JWT** — issued via client-credentials at `POST /api/auth/m2m/token` for trusted automation. Same signing key; carries scope claims drawn from `M2mClientScope`.

Refresh tokens (`Domain.Entities.Auth.RefreshToken`) are persisted and rotated on use. Default validity `OAuth:Jwt:RefreshTokenExpirationDays` (30).

`Web.Api.Setup.Auth.SetupAuthentication` checks whether `OAuth:Jwt:Secret` is set to a non-placeholder value:

- If set, the API self-validates JWTs (`ValidateIssuer`, `ValidateAudience`, `ValidateLifetime`, `ClockSkew=Zero`).
- If not set, it falls back to `IdentityServer:Authority`/`Audience` validation. Production runs the first branch; the placeholder string is `YOUR_JWT_SECRET_KEY_MIN_32_CHARACTERS_LONG`.

## OAuth flow

`AuthController` exposes:

- `GET /api/auth/google` and `GET /api/auth/github` — generate CSRF state, store it in the session along with the optional `frontendReturnUrl` (validated against `Frontend:AllowedRedirectOrigins`), and redirect to the provider.
- `GET /api/auth/google/callback` and `GET /api/auth/github/callback` — verify state, exchange `code`, materialise or look up the user, then redirect to the frontend with `access_token`, `refresh_token`, `expires_in` query parameters.

The provider clients live in `src/Infrastructure/Authentication/OAuth/` and are selected by `OAuthProviderFactory`.

## Password flow

`User` carries password hash, password-reset token (with expiration), email-verification token (with expiration), failed-login counter, and a lockout window. `LoginHandler` and `RegisterHandler` enforce:

- BCrypt password hashing (`PasswordHasher`, package `BCrypt.Net-Next`).
- Account lockout after **5 failed attempts** for **15 minutes**.
- Anti-bot rules: a hidden `Website` honeypot field must be empty; `FormDurationSeconds` must be ≥ 1 second on login and ≥ 2 seconds on register. Failures look identical to invalid credentials — they throw `UnauthorizedException` (login) or `BadRequestException` (register) rather than a distinct error. See `changelog.md` 2026-03-28 for rationale.

Email verification is required before login; the verification link points to `GET /api/auth/verify-email/{token}` which then redirects to `Frontend:BaseUrl/login?verified=true`.

## Authorization filters

Two custom filter attributes guard endpoints:

- `[HasAnyRole(params Role[])]` (`src/Web.Api/Setup/Attributes/HasAnyRoleAttribute.cs`) — requires at least one claim and, when roles are passed, at least one matching role. `Role` is a `[Flags]`-style enum of `Undefined`, `Interviewer`, `Admin`. Failing requests throw `AuthenticationException` (no claims) or `NoPermissionsException` (claims, wrong role) — both are mapped to HTTP responses by `ExceptionHttpMiddleware`.
- `[RequiresScope(...)]` (`src/Web.Api/Setup/Attributes/RequiresScopeAttribute.cs`) — used on M2M endpoints; checks the scope claim against the route's required scope.

`IAuthorization` (`src/Infrastructure/Authentication/AuthorizationService.cs`) is the runtime façade. `GetCurrentUserOrFailAsync()` looks the user up by email or `IdentityId`, creates one on first sight, refreshes their `LastLoginAt`, and caches the result for the request.

## Sessions and CSRF

OAuth state is stored in ASP.NET Core's session middleware. Cookies are configured `HttpOnly`, `SameSite=None`, `SecurePolicy=Always`, idle timeout 30 minutes (`Startup.ConfigureServices`). The frontend redirect target is validated against `Frontend:AllowedRedirectOrigins` to prevent open-redirect after callback.

## What can be anonymous

`[AllowAnonymous]` on auth endpoints, sitemap, RSS feeds, public surveys list, public salary endpoints. Everything else falls back to JWT bearer.

## Configuration keys

| Key | Purpose |
|---|---|
| `OAuth:Google:ClientId` / `ClientSecret` / `RedirectUri` | Google OAuth |
| `OAuth:GitHub:ClientId` / `ClientSecret` / `RedirectUri` | GitHub OAuth |
| `OAuth:Jwt:Secret` | HMAC signing key for internal JWTs |
| `OAuth:Jwt:Issuer` / `Audience` | Token validation parameters |
| `OAuth:Jwt:AccessTokenExpirationMinutes` / `M2mTokenExpirationMinutes` / `RefreshTokenExpirationDays` | Lifetimes |
| `Frontend:BaseUrl` / `CallbackUrl` / `AllowedRedirectOrigins` | Post-OAuth redirect destinations |
| `IdentityServer:Authority` / `Audience` | Fallback when `OAuth:Jwt:Secret` is the placeholder |

In production the `sed` substitution in `.github/workflows/deploy.yml` writes secrets directly into `appsettings.Production.json` at build time. See `docs/gotchas.md`.
