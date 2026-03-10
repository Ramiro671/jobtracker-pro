# Day 08 — FluentValidation + Global Error Handler

**Date:** March 10, 2026
**Phase:** 1 — Backend Core
**Block:** Bloque 10 — FluentValidation + Global Error Handling Middleware
**Duration:** ~1 hour

---

## What I did

Added input validation using FluentValidation and a global exception handling
middleware that returns RFC 9110 ProblemDetails responses for all errors.

---

## Files created

### Application — Validators (2 new files)
- `JobApplications/Commands/CreateJobApplicationValidator.cs`
- `JobApplications/Commands/UpdateJobApplicationValidator.cs`

### Application — Pipeline (1 new file)
- `Common/Behaviors/ValidationBehavior.cs` — MediatR pipeline behavior

### Api — Middleware (1 new file)
- `Middleware/ExceptionHandlingMiddleware.cs` — global exception handler

### Modified
- `Application/Common/DependencyInjection.cs` — registered validators + pipeline behavior
- `Api/Program.cs` — registered `ExceptionHandlingMiddleware` as first middleware

---

## NuGet packages added

```bash
dotnet add src/JobTrackerPro.Application package FluentValidation
dotnet add src/JobTrackerPro.Application package FluentValidation.DependencyInjectionExtensions
```

---

## How it works

### FluentValidation flow

```
POST /api/JobApplications  (invalid body)
    ↓
ExceptionHandlingMiddleware
    ↓
JobApplicationsController
    ↓ ISender.Send(CreateJobApplicationCommand)
MediatR Pipeline
    ↓ ValidationBehavior<CreateJobApplicationCommand, Guid>
    ↓ runs CreateJobApplicationValidator
    ↓ finds 4 failures
    ↓ throws ValidationException
ExceptionHandlingMiddleware catches it
    ↓ returns 400 Bad Request + ProblemDetails
```

The `CreateJobApplicationHandler` is **never reached** if validation fails.
No wasted DB calls, no cryptic errors.

### ValidationBehavior — MediatR Pipeline

```csharp
// Registered in DI — runs automatically before EVERY handler
services.AddTransient(
    typeof(IPipelineBehavior<,>),
    typeof(ValidationBehavior<,>));
```

MediatR calls the pipeline behaviors in order before executing the handler.
If `ValidationBehavior` throws, the handler never runs.

### ExceptionHandlingMiddleware — two error types

| Exception | HTTP Status | Response |
|-----------|-------------|----------|
| `ValidationException` | `400 Bad Request` | `ValidationProblemDetails` with field errors |
| Any other `Exception` | `500 Internal Server Error` | Generic `ProblemDetails` (no stack trace exposed) |

---

## Verified — POST with invalid body

**Request:**
```json
{
  "userId": "00000000-0000-0000-0000-000000000000",
  "title": "",
  "companyName": "",
  "jobUrl": "not-a-url",
  "source": ""
}
```

**Response — 400 Bad Request:**
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "Validation failed",
  "status": 400,
  "errors": {
    "UserId": ["UserId is required."],
    "Title": ["Job title is required."],
    "CompanyName": ["Company name is required."],
    "JobUrl": ["Job URL must be a valid URL."],
    "Source": ["Source is required."]
  }
}
```

All 5 validation errors returned in a single response. ✅

---

## Validation rules implemented

### CreateJobApplicationCommand
| Field | Rules |
|-------|-------|
| `UserId` | NotEmpty |
| `Title` | NotEmpty · MaxLength(200) |
| `CompanyName` | NotEmpty · MaxLength(100) |
| `JobUrl` | MaxLength(500) · Must be valid absolute URL (if not empty) |
| `Source` | NotEmpty · MaxLength(50) |

### UpdateJobApplicationCommand
| Field | Rules |
|-------|-------|
| `Id` | NotEmpty |
| `NewStatus` | IsInEnum (valid ApplicationStatus value) |
| `Notes` | MaxLength(1000) |

---

## Key concepts

### Why ProblemDetails?
RFC 9110 is the HTTP standard for machine-readable error responses.
All modern APIs use it. Clients (frontend, mobile) can parse errors consistently.

```json
// Generic error — no ProblemDetails
{ "message": "Something went wrong" }

// ProblemDetails — machine readable, RFC compliant ✅
{
  "type": "https://...",
  "title": "Validation failed",
  "status": 400,
  "errors": { "Title": ["Job title is required."] }
}
```

### Why middleware and not try/catch in every controller?
```csharp
// ❌ Without middleware — repeated in every action
[HttpPost]
public async Task<IActionResult> Create(...)
{
    try { ... }
    catch (ValidationException ex) { return BadRequest(...); }
    catch (Exception ex) { return StatusCode(500, ...); }
}

// ✅ With middleware — one place handles everything
app.UseMiddleware<ExceptionHandlingMiddleware>(); // ← handles all controllers
```

**DRY principle:** Don't Repeat Yourself. One middleware, all controllers covered.

### Why ValidationBehavior and not validation in the controller?
The controller should not know about validation logic.
The Application layer owns the use cases and their rules.
MediatR pipeline behaviors are the clean architecture way to add cross-cutting concerns.

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
| **10** | **FluentValidation + Global Error Handler** | ✅ |
| 11 | JWT Auth: Setup + Register/Login | ⏳ next |

---

## Commit

```
feat: add FluentValidation and global error handling middleware
```
