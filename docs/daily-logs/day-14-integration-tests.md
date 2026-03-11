# Day 14 — Integration Tests: WebApplicationFactory

**Date:** March 10, 2026
**Phase:** 1 — Backend Core
**Block:** Bloque 17 — Integration Tests: WebApplicationFactory
**Duration:** ~1 hour

---

## What I did

Wrote 8 integration tests covering the full HTTP pipeline.
Tests run against an in-memory database — no Docker required.
All middleware, validation, auth, and controllers tested end-to-end.

---

## Test results

```
total: 8 · failed: 0 · succeeded: 8 · skipped: 0
duration: 5.3s
```

---

## Files created

```
tests/JobTrackerPro.IntegrationTests/
├── CustomWebApplicationFactory.cs     — replaces PostgreSQL with in-memory DB
├── BaseIntegrationTest.cs             — shared setup + GetAuthTokenAsync helper
├── Auth/
│   └── AuthControllerTests.cs         (4 tests)
└── JobApplications/
    └── JobApplicationsControllerTests.cs (4 tests)
```

---

## NuGet packages added

```bash
dotnet add tests/JobTrackerPro.IntegrationTests package Microsoft.AspNetCore.Mvc.Testing
dotnet add tests/JobTrackerPro.IntegrationTests package Microsoft.EntityFrameworkCore.InMemory
dotnet add tests/JobTrackerPro.IntegrationTests package FluentAssertions
```

---

## Architecture — how WebApplicationFactory works

```
Test                CustomWebApplicationFactory
 │                         │
 │  new HttpClient()        │
 │─────────────────────────▶│  Spins up real ASP.NET Core pipeline
 │                         │  Replaces PostgreSQL with InMemory DB
 │  POST /api/auth/register │
 │─────────────────────────▶│  ExceptionHandlingMiddleware
 │                         │  AuthController
 │                         │  MediatR → RegisterHandler
 │                         │  FluentValidation pipeline
 │                         │  EF Core InMemory DB
 │  200 OK { token }        │
 │◀─────────────────────────│
```

No Postman. No Docker. Real HTTP requests in milliseconds.

---

## CustomWebApplicationFactory

```csharp
protected override void ConfigureWebHost(IWebHostBuilder builder)
{
    builder.ConfigureServices(services =>
    {
        // Remove real PostgreSQL registration
        var descriptor = services.SingleOrDefault(
            d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
        if (descriptor != null) services.Remove(descriptor);

        // Replace with in-memory database
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseInMemoryDatabase("JobTrackerTestDb"));
    });
}
```

---

## Tests written

### AuthControllerTests (4 tests)

| Test | Endpoint | Expected |
|------|----------|----------|
| `Register_WithValidData_ShouldReturn200WithTokens` | POST /api/auth/register | 200 + accessToken + refreshToken |
| `Register_WithDuplicateEmail_ShouldReturn500` | POST /api/auth/register | 500 (email taken) |
| `Login_WithValidCredentials_ShouldReturn200WithTokens` | POST /api/auth/login | 200 + accessToken |
| `Login_WithWrongPassword_ShouldReturn401` | POST /api/auth/login | 401 Unauthorized |

### JobApplicationsControllerTests (4 tests)

| Test | Endpoint | Expected |
|------|----------|----------|
| `GetJobApplications_WithoutToken_ShouldReturn401` | GET /api/jobapplications/{id} | 401 |
| `CreateJobApplication_WithValidToken_ShouldReturn201` | POST /api/jobapplications | 201 + id |
| `CreateJobApplication_WithInvalidData_ShouldReturn400` | POST /api/jobapplications | 400 + validation errors |
| `GetJobApplications_AfterCreating_ShouldReturnApplications` | GET /api/jobapplications/{id} | 200 + list |

---

## Serilog output during tests — full pipeline verified

```
[INF] Registering new user with email test_21f300cd...@jobtracker.com
[INF] User 612d3d43-... registered successfully
[INF] HTTP POST /api/auth/register responded 200 in 1292ms

[WRN] Unauthorized access: Invalid email or password.
[INF] HTTP POST /api/auth/login responded 401 in 253ms

[WRN] Validation failed: ["Job title is required.", ...]
[INF] HTTP POST /api/jobapplications responded 400 in 14ms

[INF] Creating job application for user ... at company Anthropic
[INF] Job application ab391784-... created successfully
[INF] HTTP POST /api/jobapplications responded 201 in 178ms
```

Every layer worked: auth, validation, handlers, logging. ✅

---

## Unit tests vs Integration tests — final comparison

| Aspect | Unit Tests (Bloque 16) | Integration Tests (Bloque 17) |
|--------|----------------------|-------------------------------|
| What's tested | Single handler class | Full HTTP pipeline |
| Dependencies | All mocked with Moq | Real middleware + real EF Core |
| Database | None | EF Core InMemory |
| Speed | ~0.1s per test | ~0.5s per test |
| What they catch | Logic bugs | Routing bugs, middleware bugs, DI errors |
| Files | `UnitTests/` | `IntegrationTests/` |

Both are needed. Unit tests catch logic bugs fast. Integration tests catch wiring bugs.

---

## Note — MediatR license warning

```
WRN: You do not have a valid license key for MediatR.
     This is allowed for development and testing scenarios.
```

MediatR recently added a commercial license requirement for production.
For a portfolio project (non-production) → no action needed.
For production deployment → check https://luckypennysoftware.com for licensing.

---

## Phase 1 — COMPLETE ✅

| Block | Content | Status |
|-------|---------|--------|
| 1 | Clean Architecture setup | ✅ |
| 2-3 | Domain: Entities + Interfaces | ✅ |
| 4 | Application: MediatR + DTOs | ✅ |
| 5 | Infrastructure: Repositories + DI | ✅ |
| 6 | API: Controller + Swagger | ✅ |
| 7 | Docker + PostgreSQL + Migrations | ✅ |
| 8 | Repository Pattern | ✅ |
| 9 | CRUD: PUT + DELETE | ✅ |
| 10 | FluentValidation + Error Handler | ✅ |
| 11 | JWT Auth: Register + Login | ✅ |
| 12 | Refresh Token + [Authorize] | ✅ |
| 13 | Clean Architecture Audit | ✅ |
| 14 | Serilog Structured Logging | ✅ |
| 15 | README en inglés | ✅ |
| 16 | Unit Tests: xUnit + Moq | ✅ |
| **17** | **Integration Tests: WebApplicationFactory** | ✅ |

**Phase 1 backend complete. Next: Phase 2 — Docker + Redis + CI/CD.**

---

## Commit

```
test: add integration tests with WebApplicationFactory and in-memory database
```
