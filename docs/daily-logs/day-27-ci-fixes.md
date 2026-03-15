# Day 27 — CI Bug Fixes

**Date:** 2026-03-15
**Phase:** Phase 4 — DevOps & Cloud
**Session focus:** Fix 8 failing integration tests and CI Node.js deprecation warnings

---

## What I did today

### 1. Root cause: integration tests failing with empty-body 429

**Symptom:** 8 of 16 integration tests failing with:

```
System.Text.Json.JsonException: The input does not contain any JSON tokens.
```

The stack trace pointed to `BaseIntegrationTest.GetAuthTokenAsync` at line 37
(`response.Content.ReadFromJsonAsync<AuthResponse>()`).

**Diagnosis:**

All tests share one `CustomWebApplicationFactory` instance (via `[Collection("Integration")]`),
which means all HTTP requests come from the same IP address. The `AuthController` had:

```csharp
[EnableRateLimiting("auth")]
```

And the rate limiter was configured as:

```csharp
options.AddFixedWindowLimiter("auth", policy =>
{
    policy.PermitLimit = 10;   // ← only 10 requests per minute
    policy.Window = TimeSpan.FromMinutes(1);
    ...
});
```

With 16 tests each calling `GetAuthTokenAsync` (→ POST `/api/auth/register`), the 11th request
onwards received `429 Too Many Requests` with an empty body. Trying to deserialize that as JSON
threw the exception.

**Fix in `src/JobTrackerPro.Api/Program.cs`:**

```csharp
// Skip rate limiter entirely in Testing environment
if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddRateLimiter(options => { ... });
}

// ...

if (!app.Environment.IsEnvironment("Testing"))
    app.UseRateLimiter();
```

`[EnableRateLimiting]` attributes are silently ignored when `UseRateLimiter()` is not in the
pipeline, so no controller changes were needed.

---

### 2. PATCH /api/jobapplications/{id} returning 400 on valid requests

**Symptom:** `Edit_WithValidId_ShouldReturn204AndPersistChanges` and
`Edit_WithUnknownId_ShouldReturn404` both failed with 400 BadRequest.

**Diagnosis:**

`EditJobApplicationCommand` had `CompanyName` as a non-nullable `string`:

```csharp
public record EditJobApplicationCommand(
    Guid Id,
    string Title,
    string CompanyName,   // ← non-nullable
    string? JobUrl,
    string? Notes
) : IRequest<bool>;
```

ASP.NET Core model binding treats non-nullable reference types as required in .NET 7+.
The test sent only `{ title, jobUrl, notes }` without `companyName`, so the binder
rejected the request with 400 before it reached the handler.

The handler already handled missing company correctly:

```csharp
if (!string.IsNullOrWhiteSpace(request.CompanyName))
{
    // only reassign company if explicitly provided
}
```

**Fix in `EditJobApplicationCommand.cs`:**

```csharp
public record EditJobApplicationCommand(
    Guid Id,
    string Title,
    string? CompanyName,  // ← nullable — optional in PATCH semantics
    string? JobUrl,
    string? Notes
) : IRequest<bool>;
```

---

### 3. CI Node.js 24 opt-in

GitHub Actions deprecated Node.js 20 runners. All 4 jobs showed the warning:

```
Node.js 20 actions are deprecated. Actions will be forced to run with Node.js 24
by default starting June 2nd, 2026.
```

**Fix in `.github/workflows/ci.yml`:**

```yaml
env:
  FORCE_JAVASCRIPT_ACTIONS_TO_NODE24: true
```

Added at the top-level `env` block so it applies to all jobs.

---

## Final test results

```
Passed!  - Failed: 0, Passed: 10  - JobTrackerPro.UnitTests.dll
Passed!  - Failed: 0, Passed: 16  - JobTrackerPro.IntegrationTests.dll

Total: 26 tests — 0 failures
```

---

## Lessons learned

- **Rate limiters and shared test factories don't mix** — fixed-window limiters are per-IP,
  and `WebApplicationFactory` uses the same loopback address for all requests. Always skip
  or disable rate limiting in the Testing environment.
- **Non-nullable `string` = required in model binding** — in .NET 7+, non-nullable reference
  types in record/class parameters are implicitly required by the model binder. Use `string?`
  for any field that is optional in PATCH semantics.
- **`[EnableRateLimiting]` is a no-op without `UseRateLimiter()`** — you don't need to remove
  the attribute from controllers; just skip the middleware registration.
