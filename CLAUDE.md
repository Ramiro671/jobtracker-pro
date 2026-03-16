# JobTracker Pro — CLAUDE.md

Project context for Claude Code. Keep this file up to date as the project evolves.

---

## What is this project

A web app to track job applications through their full lifecycle (Saved → Applied → Interview → Offer → Accepted/Rejected).

- **Backend:** .NET 10, Clean Architecture (Domain / Application / Infrastructure / Api)
- **Frontend:** React + TypeScript + Vite + Tailwind CSS
- **Database:** PostgreSQL via Neon.tech (free tier, serverless)
- **Auth:** JWT + Refresh Tokens
- **Hosting:** Azure App Service (API) + GitHub Pages (Frontend)
- **CI/CD:** GitHub Actions (`.github/workflows/ci.yml`)

---

## Architecture

```
src/
  JobTrackerPro.Domain/          # Entities, Enums, Interfaces, ValueObjects
  JobTrackerPro.Application/     # MediatR Commands/Queries, DTOs, Validators, Behaviors
  JobTrackerPro.Infrastructure/  # EF Core, Repositories, JWT, Redis, Migrations
  JobTrackerPro.Api/             # Controllers, Middleware, Program.cs

frontend/
  src/
    api/          # Axios calls to the backend
    components/   # JobApplicationCard, AddApplicationModal, EditApplicationModal, ProtectedRoute
    context/      # AuthContext (JWT stored in localStorage)
    pages/        # LoginPage, RegisterPage, DashboardPage
    types/        # ApplicationStatus enum + STATUS_LABELS, STATUS_COLORS, JobApplication interface

tests/
  JobTrackerPro.UnitTests/
  JobTrackerPro.IntegrationTests/   # Uses CustomWebApplicationFactory with InMemory DB + UseEnvironment("Testing")
```

---

## Key domain concepts

### ApplicationStatus enum (backend: Domain/Enums/ApplicationStatus.cs)

| Value | Name           |
|-------|----------------|
| 0     | Saved          |
| 1     | Applied        |
| 2     | Screening      |
| 3     | TechnicalTest  |
| 4     | Interview      |
| 5     | OfferReceived  |
| 6     | Accepted       |
| 7     | Rejected       |
| 8     | Withdrawn      |

**IMPORTANT:** The frontend `ApplicationStatus` const object (types/index.ts) uses different names/values than the backend enum. They are intentionally different — do NOT try to "sync" them unless both sides are updated together and tested.

### JobApplicationDto (Application/DTOs/JobApplicationDto.cs)

`Status` is returned as `int` (not string). This was a deliberate fix — the API originally returned `"Applied"` as a string, which broke the frontend filter that compares `a.status === filter` (number). Always keep `Status` as `int` in the DTO.

`UpdatedAt` is included in the DTO to allow the frontend to compute stale application detection (7+ days no activity).

### API endpoints (JobApplicationsController)

| Method | Route | Description |
|--------|-------|-------------|
| `GET` | `/api/jobapplications/{userId}` | List all applications for user |
| `POST` | `/api/jobapplications` | Create new application |
| `PUT` | `/api/jobapplications/{id}` | Update status + notes |
| `PATCH` | `/api/jobapplications/{id}` | Edit title, jobUrl, notes |
| `DELETE` | `/api/jobapplications/{id}` | Delete application |

### Domain: UpdateStatus

`JobApplication.UpdateStatus(ApplicationStatus newStatus, string? notes = null)` — single method with optional notes. Automatically sets `AppliedAt` when transitioning to `Applied` for the first time. Do NOT add a second overload without notes — it was removed to fix a silent bug.

---

## Infrastructure decisions

### Database
- **Neon.tech** free tier (serverless PostgreSQL). Chosen over Azure Database for PostgreSQL to avoid ~$60/month cost.
- Connection string format for Npgsql (NOT the postgresql:// URI format):
  ```
  Host=...;Database=neondb;Username=neondb_owner;Password=...;SSL Mode=Require;
  ```
- Azure App Service: added as **Connection String** named `DefaultConnection`, type `Custom`.

### EF Core migrations
- Applied automatically on startup via `db.Database.Migrate()` in Program.cs.
- Guarded with `!app.Environment.IsEnvironment("Testing")` — do NOT use `IsRelational()` as a guard (it returns `true` even for InMemory providers when using `UseInternalServiceProvider`).

### JWT
- Settings in `appsettings.json` under `JwtSettings` section.
- Azure App Service env vars use double-underscore for nested keys: `JwtSettings__Secret`, `JwtSettings__Issuer`, `JwtSettings__Audience`.

### Redis
- Used for caching via `ICacheService`.
- Local dev: `localhost:6379` (via Docker Compose).
- Not configured in production yet — falls back to default if connection string is missing.

---

## Frontend

### Auth
- JWT stored in `localStorage` (keys: `accessToken`, `userId`).
- `AuthContext` provides `userId`, `login`, `logout`.
- `ProtectedRoute` wraps authenticated pages.

### API base URL
- Dev: `http://localhost:5000` (via `frontend/.env.development` or Vite default)
- Prod: set via `VITE_API_URL` GitHub Secret → injected at build time → `frontend/.env.production`
- Current prod URL: `https://jobtracker-api-prod-ehg6euckd4evaabw.centralus-01.azurewebsites.net`

### SPA routing
- GitHub Pages SPA routing is handled by `frontend/public/404.html` — GitHub Pages serves it on unknown paths; it redirects to `index.html` with the path as a query string, which the app then restores.
- `BrowserRouter` has `basename="/jobtracker-pro"` to match the GitHub Pages sub-path.
- `vite.config.ts` has `base: '/jobtracker-pro/'` for correct asset paths.

---

## CI/CD pipeline (.github/workflows/ci.yml)

Triggered on push/PR to `main`. Jobs in order:

1. **build-and-test** — `dotnet build` + unit tests + integration tests
2. **docker-build** — builds Docker image (no push, just validates)
3. **deploy-api** — `dotnet publish` → Azure Web App (`azure/webapps-deploy@v3`)
4. **deploy-frontend** — `npm ci && npm run build` → GitHub Pages (`peaceiris/actions-gh-pages@v4`)

### Required GitHub Secrets

| Secret | Used for |
|--------|----------|
| `AZURE_APP_NAME` | Azure Web App name |
| `AZURE_PUBLISH_PROFILE` | Azure deployment credentials |
| `GITHUB_TOKEN` | GitHub Pages deploy (built-in, no manual setup) |
| `VITE_API_URL` | Frontend API URL injected at build time |

---

## CORS

Allowed origins in `Program.cs`:
- `http://localhost:5173`
- `http://localhost:5174`
- `https://ramiro671.github.io`

When the GitHub Pages domain changes, update Program.cs and redeploy.

---

## Local development

```bash
# Backend (from repo root)
docker compose up -d        # starts PostgreSQL + Redis
dotnet run --project src/JobTrackerPro.Api

# Frontend
cd frontend
npm install
npm run dev
```

Swagger UI available at `http://localhost:5000` (root prefix is empty).

---

## Frontend features (DashboardPage)

- **Stats bar** — Total, Active, Offers counts
- **Stale warning banner** — amber alert for applications in active statuses (Applied, PhoneScreen, Interview, TechnicalTest, FinalInterview) with 7+ days no activity, computed from `updatedAt ?? createdAt`
- **Search bar** — client-side filter by title or company name
- **Status filter** — dropdown, filters by `ApplicationStatus` number
- **Cards** — show title, companyName, date added, status badge, URL link, notes preview, status dropdown, Edit button, Delete button

---

## Known issues / watch out for

- **Status enum mismatch:** Backend `ApplicationStatus` and frontend `ApplicationStatus` have different names. Backend uses `Applied=1`, frontend uses `Applied=0`. The DTO returns `int` so the frontend values are authoritative for display. Don't change the numeric values on either side without updating both.
- **appliedAt can be null:** New applications don't have `appliedAt` set. The card shows `createdAt` with "Added" label instead. Do not use `new Date(null)` — it returns Unix epoch.
- **Integration tests:** Use `UseEnvironment("Testing")` and InMemory EF. The migration guard in Program.cs relies on this. Do not change the environment name.
- **Redis not in production:** `RedisCacheService` already has graceful fallback (try/catch logs warning and returns `default`). Do not remove the try/catch blocks — they are the fallback mechanism.
- **PATCH vs PUT:** `PATCH /api/jobapplications/{id}` edits title/jobUrl/notes. `PUT /api/jobapplications/{id}` updates status+notes. Keep them separate — they map to different domain operations.
