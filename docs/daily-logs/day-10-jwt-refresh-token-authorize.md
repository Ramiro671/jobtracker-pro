# Day 10 — JWT: Refresh Token + [Authorize] + Swagger Bearer UI

**Date:** March 10, 2026
**Phase:** 1 — Backend Core
**Block:** Bloque 12 — JWT: Token Generation + Refresh Token
**Duration:** ~1 hour

---

## What I did

Extended JWT authentication with refresh tokens, protected endpoints with
[Authorize], fixed 401 error handling, and added Bearer token support in Swagger UI.

---

## Files created

### Domain (1 new entity + 1 new interface)
- `Entities/RefreshToken.cs` — entity with Token, UserId, ExpiresAt, IsRevoked, IsActive
- `Interfaces/IRefreshTokenRepository.cs` — GetByTokenAsync + AddAsync

### Infrastructure (1 new repository)
- `Persistence/Repositories/RefreshTokenRepository.cs` — EF Core implementation
- `Persistence/ApplicationDbContext.cs` — added `DbSet<RefreshToken>`
- `DependencyInjection.cs` — registered `IRefreshTokenRepository`

### Application (3 new files + 2 updated)
- `Auth/DTOs/AuthResponse.cs` — record(AccessToken, RefreshToken, ExpiresAt)
- `Auth/Commands/RefreshTokenCommand.cs` — IRequest<AuthResponse>
- `Auth/Commands/RefreshTokenHandler.cs` — validates + revokes old + issues new
- `Auth/Commands/RegisterHandler.cs` — updated to return AuthResponse
- `Auth/Commands/LoginHandler.cs` — updated to return AuthResponse

### Api (2 updated)
- `Controllers/AuthController.cs` — added POST /api/auth/refresh
- `Controllers/JobApplicationsController.cs` — added [Authorize]
- `Middleware/ExceptionHandlingMiddleware.cs` — added UnauthorizedAccessException → 401
- `Program.cs` — updated Swagger with Bearer security definition

### Database
```bash
dotnet ef migrations add AddRefreshTokens \
  --project src/JobTrackerPro.Infrastructure \
  --startup-project src/JobTrackerPro.Api

dotnet ef database update \
  --project src/JobTrackerPro.Infrastructure \
  --startup-project src/JobTrackerPro.Api
```

---

## AuthResponse — new response format

Before (Bloque 11):
```json
{ "token": "eyJhbGci..." }
```

After (Bloque 12):
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "oF7/8BEykkWAX6I/1drTjA==",
  "expiresAt": "2026-03-10T21:48:53.1246825Z"
}
```

---

## Refresh Token flow

```
1. POST /api/auth/login → { accessToken (60 min), refreshToken (7 days) }

2. Client uses accessToken for all requests:
   Authorization: Bearer eyJhbGci...

3. accessToken expires after 60 minutes

4. Client sends POST /api/auth/refresh { token: "oF7/8BEy..." }
   → RefreshTokenHandler:
     - Finds RefreshToken in DB
     - Validates IsActive (not revoked, not expired)
     - Revokes old token (IsRevoked = true)
     - Creates new RefreshToken
     - Issues new accessToken
   → { new accessToken, new refreshToken, new expiresAt }

5. Cycle repeats for up to 7 days without re-login
```

---

## RefreshToken entity design

```csharp
public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
public bool IsActive => !IsRevoked && !IsExpired;

public void Revoke() => IsRevoked = true;
```

**Token rotation:** Every refresh issues a new token and revokes the old one.
If a stolen token is used after rotation → it's already revoked → 401 Unauthorized.

---

## [Authorize] — protected endpoints

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize] // ← all endpoints in this controller require valid JWT
public class JobApplicationsController : ControllerBase { ... }
```

Without token: `401 Unauthorized`
With invalid token: `401 Unauthorized`
With valid token: `200 OK` ✅

---

## Middleware fix — UnauthorizedAccessException → 401

Before: Login with wrong password → `500 Internal Server Error`
After: Login with wrong password → `401 Unauthorized`

```csharp
catch (UnauthorizedAccessException ex)
{
    _logger.LogWarning("Unauthorized access: {Message}", ex.Message);
    await HandleUnauthorizedExceptionAsync(context, ex);
}
```

Response:
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.2",
  "title": "Unauthorized",
  "detail": "Invalid email or password.",
  "status": 401
}
```

---

## Swagger Bearer UI

Added security definition so Swagger can send authenticated requests:

```csharp
c.AddSecurityDefinition("Bearer", new()
{
    Name = "Authorization",
    Type = SecuritySchemeType.Http,
    Scheme = "Bearer",
    BearerFormat = "JWT",
    In = ParameterLocation.Header,
    Description = "Enter your JWT token."
});
```

Usage: Click **Authorize** button → paste `accessToken` → all requests include `Authorization: Bearer ...` header automatically.

---

## Verified — end-to-end

```
POST /api/Auth/register
  → 200 OK { accessToken, refreshToken, expiresAt } ✅

POST /api/Auth/login (correct)
  → 200 OK { accessToken, refreshToken, expiresAt } ✅

POST /api/Auth/login (wrong password)
  → 401 Unauthorized { title: "Unauthorized", detail: "Invalid email or password." } ✅

GET /api/JobApplications/{userId} (no token)
  → 401 Unauthorized ✅

GET /api/JobApplications/{userId} (with Bearer token)
  → 200 OK [] ✅

POST /api/Auth/refresh { token: "oF7/8BEy..." }
  → 200 OK { new accessToken, new refreshToken } ✅
```

---

## Phase 1 progress

| Block | Content | Status |
|-------|---------|--------|
| 1 | Clean Architecture setup | ✅ |
| 2-3 | Domain: Entities + Interfaces | ✅ |
| 4 | Application: MediatR + DTOs | ✅ |
| 5 | Infrastructure: Repositories + DI | ✅ |
| 6 | API: Controller + Swagger | ✅ |
| 7 | Docker + PostgreSQL + Migrations | ✅ |
| 8 | Repository Pattern: Full implementation | ✅ |
| 9 | CRUD Completo: PUT + DELETE | ✅ |
| 10 | FluentValidation + Global Error Handler | ✅ |
| 11 | JWT Auth: Register + Login | ✅ |
| **12** | **Refresh Token + [Authorize] + Swagger Bearer** | ✅ |
| 13 | Clean Architecture Audit + Refactor | ⏳ next |

---

## Commits

```
feat: add refresh tokens, [Authorize] and Swagger Bearer UI
```
