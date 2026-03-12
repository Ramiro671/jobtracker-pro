# Day 22 — CI/CD Complete + Azure + Netlify Deploy (Bloques 25–28)

**Date:** March 12, 2026
**Phase:** 4 — Deploy
**Blocks:** 25 · 26 · 27 · 28
**Duration:** ~3 hours

---

## What I did

Completed the full CI/CD pipeline, deployed the API to Azure App Service,
deployed the frontend to Netlify, and integrated Application Insights monitoring.

---

## Result

```
GitHub Actions CI/CD #5 — Status: Success ✅
✅ Build and Test      1m 26s  (18/18 tests)
✅ Docker Build        43s
✅ Deploy API to Azure 1m 5s
✅ Deploy Frontend     20s
Total duration: 3m 24s
```

---

## BLOQUE 25 — GitHub Actions CI/CD Complete

Updated `.github/workflows/ci.yml` — 4 jobs in pipeline:

```
build-and-test → docker-build → deploy-api    (on push to main only)
build-and-test →               deploy-frontend (on push to main only)
```

Added secrets to GitHub repository:

| Secret | Purpose |
|--------|---------|
| `AZURE_APP_NAME` | jobtracker-api-prod |
| `AZURE_PUBLISH_PROFILE` | XML from Azure Portal Download |
| `VITE_API_URL` | Production API URL |
| `NETLIFY_AUTH_TOKEN` | Netlify deploy token |
| `NETLIFY_SITE_ID` | Netlify site identifier |

---

## BLOQUE 26 — Azure App Service

### Resources created

| Resource | Value |
|----------|-------|
| Resource Group | jobtracker-rg |
| App Service | jobtracker-api-prod |
| Region | Central US (East US had quota 0 for Free tier) |
| Runtime | .NET 10 (LTS) |
| OS | Linux |
| SKU | Free F1 (Shared infrastructure) |

### Environment variables configured in Azure Portal

```
ASPNETCORE_ENVIRONMENT          = Production
JwtSettings__Issuer             = JobTrackerPro
JwtSettings__Audience           = JobTrackerPro
JwtSettings__ExpirationMinutes  = 60
JwtSettings__Secret             = [redacted — set in Azure Portal]
```

### Lesson learned

East US region had `Current Limit (Free VMs): 0` — switched to Central US.
Azure new accounts can have quota 0 for Free tier in certain regions.

---

## BLOQUE 27 — Netlify Frontend Deploy

Created `frontend/netlify.toml`:

```toml
[build]
  base = "frontend"
  publish = "dist"
  command = "npm run build"

[build.environment]
  NODE_VERSION = "22"

[[redirects]]
  from = "/*"
  to = "/index.html"
  status = 200
```

The `/*` → `/index.html` redirect is critical for React Router.
Without it, refreshing `/dashboard` returns 404 from Netlify's CDN.

---

## BLOQUE 28 — Application Insights

Added `Microsoft.ApplicationInsights.AspNetCore` package.

`Program.cs` guard added:

```csharp
var aiConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
if (!string.IsNullOrEmpty(aiConnectionString))
{
    builder.Services.AddApplicationInsightsTelemetry(options =>
    {
        options.ConnectionString = aiConnectionString;
    });
}
```

Without the guard: `AddApplicationInsightsTelemetry` with an empty connection string
threw `InvalidOperationException` during host startup. `Program.cs` `try/catch`
silently swallowed it. The `DeferredHostBuilder` got a disposed host — this was
the root cause of `ObjectDisposedException: IServiceProvider` in integration tests.

`appsettings.json` — empty in dev (connection string goes in Azure App Service Configuration):
```json
"ApplicationInsights": {
  "ConnectionString": ""
}
```

---

## Bug fixed — WebApplicationFactory .NET 10 compatibility

### Root cause analysis

| Problem | Root cause |
|---------|-----------|
| `ObjectDisposedException: IServiceProvider` | `AddApplicationInsightsTelemetry` with empty string → host startup exception → GC-disposed `DeferredHostBuilder` |
| 500 on all routes after restoring InMemory | Npgsql + InMemory in same DI container — wrong EF provider resolved |
| Extra factory instances | xUnit 2.x recognizes `IClassFixture<T>` from base classes — triggered Serilog double-init |

### Fix applied

`CustomWebApplicationFactory.cs` — added `IAsyncLifetime`:
```csharp
public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public new async Task InitializeAsync() => await base.InitializeAsync();
    public new async Task DisposeAsync()    => await base.DisposeAsync();
}
```

`Program.cs` — guarded `AddApplicationInsightsTelemetry` with null check.

Result: 18/18 tests passing in CI. ✅

---

## Commits

```
ci: add full CI/CD pipeline with Azure and Netlify deploy
feat: add Application Insights monitoring
fix: initialize WebApplicationFactory for .NET 10 compatibility
```
