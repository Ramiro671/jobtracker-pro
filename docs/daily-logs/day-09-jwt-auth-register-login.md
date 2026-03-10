# Day 09 — JWT Authentication: Register + Login

**Date:** March 10, 2026
**Phase:** 1 — Backend Core
**Block:** Bloque 11 — JWT Auth: Setup + Register/Login
**Duration:** ~1 hour

---

## What I did

Implemented full JWT authentication with user registration and login.
Passwords are hashed with BCrypt. Tokens are signed with HS256.

---

## Files created

### Infrastructure — Authentication (3 new files)
- `Authentication/JwtSettings.cs` — options pattern bound from appsettings.json
- `Authentication/IJwtTokenGenerator.cs` — interface for token generation
- `Authentication/JwtTokenGenerator.cs` — signs JWT tokens with HS256

### Application — Auth (4 new files)
- `Auth/Commands/RegisterCommand.cs` — IRequest<string>
- `Auth/Commands/RegisterHandler.cs` — creates user + hashes password + returns token
- `Auth/Commands/LoginCommand.cs` — IRequest<string>
- `Auth/Commands/LoginHandler.cs` — validates credentials + returns token

### Api (1 new file)
- `Controllers/AuthController.cs` — POST /api/auth/register · POST /api/auth/login

### Modified
- `appsettings.json` — added JwtSettings section
- `Infrastructure/DependencyInjection.cs` — registered JwtSettings + JwtTokenGenerator + JWT auth
- `Api/Program.cs` — added UseAuthentication() + UseAuthorization()

---

## NuGet packages added

```bash
dotnet add src/JobTrackerPro.Infrastructure package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add src/JobTrackerPro.Infrastructure package BCrypt.Net-Next
dotnet add src/JobTrackerPro.Api package Microsoft.AspNetCore.Authentication.JwtBearer
```

---

## Configuration — appsettings.json

```json
"JwtSettings": {
  "Secret": "super-secret-key-for-development-only-change-in-production",
  "Issuer": "JobTrackerPro",
  "Audience": "JobTrackerPro",
  "ExpirationMinutes": 60
}
```

> ⚠️ Secret must be changed per environment. Never hardcode in production.
> Use environment variables or Azure Key Vault in production.

---

## How JWT works in this project

```
1. User sends POST /api/auth/register { fullName, email, password }
        ↓
2. RegisterHandler:
   - Checks email is not already registered
   - BCrypt.HashPassword(password) → stores hash (never plain text)
   - User.Create(fullName, email, hash) → saves to PostgreSQL
   - JwtTokenGenerator.GenerateToken(user) → returns signed JWT
        ↓
3. Client receives { "token": "eyJhbGci..." }
        ↓
4. Client sends token in every request:
   Authorization: Bearer eyJhbGci...
        ↓
5. API validates token on every request (next block: [Authorize])
```

---

## What is a JWT token?

A JWT (JSON Web Token) has 3 parts separated by dots:

```
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9        ← Header (Base64)
.eyJzdWIiOiJiMTE1NTA4Zi0...                  ← Payload/Claims (Base64)
.qj94zNRF8-Oi5ZvdyHxosZh369mlD1zdeUA7e9fjON0 ← Signature (HS256)
```

**Header** — algorithm used:
```json
{ "alg": "HS256", "typ": "JWT" }
```

**Payload** — the claims (data inside the token):
```json
{
  "sub": "b115508f-7d93-40de-93fa-f83b4d66aa4b",  ← User ID
  "email": "ramiro@jobtracker.com",
  "given_name": "Ramiro López",
  "jti": "37e9da4c-...",                           ← Unique token ID
  "exp": 1773171989,                               ← Expiration (Unix timestamp)
  "iss": "JobTrackerPro",                          ← Who issued it
  "aud": "JobTrackerPro"                           ← Who it's for
}
```

**Signature** — proves the token was not tampered with:
```
HMACSHA256(base64(header) + "." + base64(payload), SECRET_KEY)
```

If anyone modifies the payload, the signature won't match → token rejected.

**Key insight:** The payload is NOT encrypted — just Base64 encoded.
Anyone can read it. Never put passwords or sensitive data in JWT claims.
The signature only proves authenticity, not secrecy.

---

## BCrypt — password hashing

```csharp
// Register — hash the password before saving
var passwordHash = BCrypt.Net.BCrypt.HashPassword("Password123!");
// → "$2a$11$rJ8K2mNpQ1vL3xY..." (irreversible hash)

// Login — verify without knowing the original password
bool isValid = BCrypt.Net.BCrypt.Verify("Password123!", storedHash);
// → true ✅

bool isValid = BCrypt.Net.BCrypt.Verify("wrongpassword", storedHash);
// → false ❌
```

BCrypt includes a random **salt** in every hash — same password produces
different hashes each time. This defeats rainbow table attacks.

---

## Verified — end-to-end test

```
POST /api/Auth/register
  body: { fullName, email, password }
  → 200 OK { "token": "eyJhbGci..." } ✅

POST /api/Auth/login (correct credentials)
  → 200 OK { "token": "eyJhbGci..." } ✅

POST /api/Auth/login (wrong password)
  → 500 Internal Server Error ⚠️
  (will be fixed to 401 Unauthorized in Bloque 12)
```

---

## What's pending (Bloque 12)

| Item | Why |
|------|-----|
| Login incorrect → 401 not 500 | `UnauthorizedAccessException` needs its own handler in middleware |
| Refresh token | Short-lived access tokens + long-lived refresh tokens |
| `[Authorize]` on JobApplicationsController | Protect endpoints — require valid JWT |
| Swagger Bearer auth UI | Test protected endpoints from Swagger |

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
| **11** | **JWT Auth: Register + Login** | ✅ |
| 12 | JWT: Token + Refresh + [Authorize] | ⏳ next |

---

## Commit

```
feat: add JWT authentication with register and login endpoints
```
