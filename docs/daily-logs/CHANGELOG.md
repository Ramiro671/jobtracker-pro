# Changelog

All notable changes to JobTracker Pro are documented in this file.
Format based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).
Versioning follows [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [1.0.0] - 2026-03-11

### Added
- Clean Architecture solution with Domain, Application, Infrastructure, and API layers
- Domain entities: JobApplication, Company, User, RefreshToken
- ApplicationStatus enum with 9 stages (Applied → Withdrawn)
- CQRS pattern with MediatR: commands and queries for all use cases
- FluentValidation pipeline behavior for input validation
- Global exception handling middleware (RFC 9110 ProblemDetails)
- JWT authentication: register, login, and refresh token endpoints
- BCrypt password hashing
- Token rotation: refresh tokens are revoked on use
- Full CRUD REST API: GET, POST, PUT, DELETE for job applications
- PostgreSQL 16 database with Entity Framework Core migrations
- Redis distributed cache with cache-aside pattern (10 min TTL)
- Serilog structured logging with console and rolling file sinks
- Docker multi-stage build (SDK → runtime, 106MB final image)
- Docker Compose stack: PostgreSQL + Redis + API
- React 18 + TypeScript frontend with Vite and Tailwind CSS
- Authentication UI: Login and Register pages
- Dashboard with stats: Total, Active, Offers
- Job application cards with inline status updates
- Add application modal with form validation
- Protected routes with JWT context
- GitHub Actions CI/CD pipeline (build → test → docker → deploy)
- Azure App Service deployment configuration
- Netlify frontend deployment with React Router redirect support
- Application Insights monitoring integration
- 8 unit tests (xUnit + Moq + FluentAssertions)
- 8 integration tests (WebApplicationFactory + EF Core InMemory)
- Swagger/OpenAPI with Bearer token authentication UI
- XML documentation comments on all public APIs

### Security
- Passwords never stored in plain text (BCrypt with salt)
- JWT tokens expire after 60 minutes
- Refresh tokens expire after 7 days with rotation
- Non-root Docker container user
- Secrets managed via environment variables (never in source control)
- [Authorize] attribute on all protected endpoints

---

## [1.1.0] - 2026-03-14

### Added
- **Dark mode** — class-based dark mode toggle with ThemeContext; persisted in localStorage; all components updated with Tailwind `dark:` variants
- **CSV export** — client-side export of all applications to `.csv`; no dependencies (Blob + createObjectURL)
- **Change password** — `PUT /api/users/me/password` endpoint; `ChangePasswordCommand`, `ChangePasswordHandler`, `UsersController`; verifies current password with BCrypt before updating
- **Automatic JWT refresh** — Axios response interceptor queues concurrent 401 requests while refreshing; retries all on success; redirects to `/login` on failure
- **Stale email notifications** — `StaleNotificationService` (BackgroundService) runs every 24h; detects applications with 7+ days no activity; sends email per user via `SmtpEmailService` (disabled by default, logs when off)
- **EmailSettings** configuration section in `appsettings.json` with `Enabled: false` default
- **`IEmailService` / `SmtpEmailService`** — infrastructure email abstraction with graceful no-op when disabled
- **Interactive visual study guide** — `docs/daily-logs/interactive-guide.html` (standalone HTML, 10 modules, animated request flow)
- **Study guide prompt** — `docs/daily-logs/study-guide-prompt.txt` for generating a bilingual study guide via Claude Chat

### Changed
- `JobApplicationDto` now includes `WorkModality`, `SeniorityLevel`, `Source`, `SalaryMin`, `SalaryMax`, `SalaryCurrency`
- Dashboard: search bar, edit modal (PATCH), stale banner (amber alert, 7-day threshold), pagination (12 per page)
- Integration tests expanded from 8 to 16 cases (12 JobApplications + 4 Auth); unit tests from 8 to 10

### Security
- Rate limiting added to `UsersController` (`[EnableRateLimiting("api")]`)
- Change password verifies current password before hashing new one

---

## [1.1.1] - 2026-03-15

### Fixed
- **Integration tests: rate limiter** — `AddRateLimiter`/`UseRateLimiter` now skipped when `IsEnvironment("Testing")`. The "auth" fixed-window limiter (10 req/min) was rejecting requests after the 10th auth call in `WebApplicationFactory`, causing 8 of 16 tests to fail with empty-body 429 responses.
- **`EditJobApplicationCommand.CompanyName` nullable** — changed from `string` to `string?`. ASP.NET Core model binding was returning 400 when the field was omitted in PATCH requests; the handler already guarded with `IsNullOrWhiteSpace`.

### CI/CD
- Opted into Node.js 24 for all GitHub Actions runners (`FORCE_JAVASCRIPT_ACTIONS_TO_NODE24: true`) to silence deprecation warnings ahead of the June 2026 forced migration.

---

## [Unreleased]

### Planned
- Surface `Description`, `TechStack`, `ContactName`, `ContactEmail` fields in API and frontend
- Application statistics charts
- Mobile responsive improvements
- LinkedInAgent.Grpc integration — auto-fill job applications from LinkedIn scraper
