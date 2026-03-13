# Day 24 — Production: Database, CORS, and Bug Fixes

**Date:** 2026-03-13
**Phase:** Phase 4 — DevOps & Cloud
**Session focus:** Full production debugging — database provisioning, CORS, CI/CD secrets, and three frontend bugs

---

## What I did today

### 1. Frontend CI/CD — Netlify secrets

The deploy job was completing successfully (`vite build` passed) but the Netlify step logged:

```
Netlify credentials not provided, not deployable
```

Root cause: `NETLIFY_AUTH_TOKEN` and `NETLIFY_SITE_ID` were missing from GitHub repository secrets.

**Fix:**
- Generated a Personal Access Token in Netlify → User settings → Personal access tokens
- Added `NETLIFY_AUTH_TOKEN` and `NETLIFY_SITE_ID` to GitHub → Settings → Secrets and variables → Actions
- Added `VITE_API_URL` secret pointing to the real Azure URL

After adding the secrets, the deploy job connected to Netlify and deployed successfully.

---

### 2. CORS — Netlify origin not allowed

After the frontend deployed, register calls failed:

```
Access to XMLHttpRequest at 'https://jobtracker-api-prod-...azurewebsites.net/api/auth/register'
from origin 'https://gleaming-lollipop-3b4183.netlify.app' has been blocked by CORS policy
```

The API only allowed `localhost:5173` and `localhost:5174`.

**Fix in `src/JobTrackerPro.Api/Program.cs`:**

```csharp
builder.Services.AddCors(options =>
    options.AddPolicy("Frontend", policy =>
        policy.WithOrigins(
            "http://localhost:5173",
            "http://localhost:5174",
            "https://gleaming-lollipop-3b4183.netlify.app"  // ← added
        )
        .AllowAnyHeader()
        .AllowAnyMethod()));
```

---

### 3. Database — Azure PostgreSQL vs Neon.tech

Azure Database for PostgreSQL Flexible Server (B2s, 2 vCores) showed an estimated cost of **$60.25/month** — too expensive for a portfolio project.

**Decision:** Use Neon.tech free tier (serverless PostgreSQL, no expiry).

**Steps:**
1. Created a project on neon.tech with PostgreSQL 17, Azure East US region
2. Copied the pooled connection string:
   ```
   postgresql://neondb_owner:***@ep-crimson-thunder-a8wyxwlv-pooler.eastus2.azure.neon.tech/neondb?sslmode=require
   ```
3. Converted to Npgsql format:
   ```
   Host=ep-crimson-thunder-a8wyxwlv-pooler.eastus2.azure.neon.tech;Database=neondb;Username=neondb_owner;Password=***;SSL Mode=Require;
   ```
4. Added `DefaultConnection` in Azure App Service → Configuration → Connection strings (Type: Custom)

Also configured in Azure App Service → Application settings:

| Key | Value |
|-----|-------|
| `JwtSettings__Secret` | 32+ char random string |
| `JwtSettings__Issuer` | Azure App Service URL |
| `JwtSettings__Audience` | Netlify URL |
| `ASPNETCORE_ENVIRONMENT` | `Production` |

---

### 4. EF Core auto-migrate on startup

Added automatic migration execution so Neon creates the schema on first boot without manual `dotnet ef database update`.

**`Program.cs`:**

```csharp
using Microsoft.EntityFrameworkCore;

// After app.Build():
if (!app.Environment.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}
```

**Bugs encountered and fixed:**

| Error | Cause | Fix |
|-------|-------|-----|
| `CS0117: 'DatabaseFacade' does not contain a definition for 'Migrate'` | Missing `using Microsoft.EntityFrameworkCore;` | Added the using directive |
| `Relational-specific methods can only be used when the context is using a relational database provider` | `IsRelational()` returned `true` even in InMemory tests because `UseInternalServiceProvider` affects provider resolution | Replaced `IsRelational()` guard with `!app.Environment.IsEnvironment("Testing")` |

---

### 5. Bug: Date showing "31/12/1969"

New job applications displayed "Applied 31/12/1969" — the Unix epoch date.

**Root cause:** `appliedAt` is `null` on newly created applications (it gets set later when the status changes to _Applied_). `new Date(null)` in JavaScript returns the epoch.

**Fix in `frontend/src/components/JobApplicationCard.tsx`:**

```tsx
// Before:
Applied {new Date(application.appliedAt).toLocaleDateString()}

// After:
Added {new Date(application.createdAt).toLocaleDateString()}
```

Also added `createdAt: string` to the `JobApplication` TypeScript interface.

---

### 6. Bug: SPA 404 on page refresh

Refreshing `/dashboard` returned Netlify's "Page not found" error. Netlify was serving 404 for any path that didn't map to a physical file.

**Root cause:** The `netlify.toml` redirect rule (`/* → /index.html`) existed in `frontend/netlify.toml` but Vite does not copy root-level config files into `dist/`. Netlify reads `netlify.toml` from the published directory.

**Fix:** Created `frontend/public/netlify.toml` — Vite copies everything in `public/` to `dist/`:

```toml
[[redirects]]
  from = "/*"
  to = "/index.html"
  status = 200
```

This is the standard fix for any SPA (React Router, Vue Router, etc.) deployed to Netlify.

---

### 7. Bug: Status filter not working

After creating a job application and filtering by "Applied", no results appeared — the filter reset cleared them.

**Root cause:** `JobApplicationDto.Status` was returning a `string` (`"Applied"`) from the API. The TypeScript frontend uses a numeric enum (`ApplicationStatus.Applied = 0`). The comparison `a.status === 0` always failed because `"Applied" !== 0`.

**Fix in `src/JobTrackerPro.Application/DTOs/JobApplicationDto.cs`:**

```csharp
// Before:
public record JobApplicationDto(
    ...,
    string Status,
    ...);

// After:
public record JobApplicationDto(
    ...,
    int Status,
    ...);
```

**Fix in `src/JobTrackerPro.Application/JobApplications/Queries/GetJobApplicationsHandler.cs`:**

```csharp
// Before:
Status: a.Status.ToString(),

// After:
Status: (int)a.Status,
```

---

## Commits made today

```
fix: correct production API URL and add Netlify CORS origin
feat: auto-migrate database on startup for production
fix: add missing EntityFrameworkCore using for Database.Migrate()
fix: guard EF migrations by environment instead of provider type
fix: handle nullable appliedAt and show createdAt as Added date
fix: add netlify.toml to public/ for SPA routing on page refresh
fix: return status as int in JobApplicationDto to match frontend enum
```

---

## Lessons learned

- **Neon.tech is a better choice than Azure PostgreSQL for portfolio projects** — free tier, no expiry, compatible with Npgsql
- **`netlify.toml` must be in `public/`** for Vite projects, not the project root, so it gets copied into `dist/`
- **API contracts matter** — the backend returning `string` vs `int` for an enum broke the frontend silently. Strongly-typed DTOs prevent this
- **Guard auto-migrations by environment**, not by provider type — `IsRelational()` behaves unexpectedly with `UseInternalServiceProvider`
- **`null` dates are dangerous** — always handle nullable date fields in the frontend before passing to `new Date()`

---

## Production URLs

| Resource | URL |
|----------|-----|
| API (Swagger) | https://jobtracker-api-prod-ehg6euckd4evaabw.centralus-01.azurewebsites.net |
| Frontend | https://gleaming-lollipop-3b4183.netlify.app |
| Database | Neon.tech — ep-crimson-thunder (Azure East US) |

---

## CI/CD status

```
✅ Build and Test     — 18/18 tests pass (10 unit + 8 integration)
✅ Docker Build       — jobtracker-api:latest
✅ Deploy API         — Azure App Service (Central US, Free F1)
✅ Deploy Frontend    — Netlify (gleaming-lollipop-3b4183)
```
