# Day 07 — Full CRUD: PUT + DELETE Endpoints

**Date:** March 10, 2026
**Phase:** 1 — Backend Core
**Block:** Bloque 9 — CRUD Completo: GET + PUT + DELETE
**Duration:** ~1 hour

---

## What I did

Completed the full CRUD by wiring up PUT (update status) and DELETE endpoints,
and fixed a critical bug where POST was returning 201 but never persisting data.

---

## Files created / modified

### Application (4 new files)
- `JobApplications/Commands/UpdateJobApplicationCommand.cs` — `IRequest<bool>`
- `JobApplications/Commands/UpdateJobApplicationHandler.cs` — finds by Id, calls `UpdateStatus()`, saves
- `JobApplications/Commands/DeleteJobApplicationCommand.cs` — `IRequest<bool>`
- `JobApplications/Commands/DeleteJobApplicationHandler.cs` — finds by Id, calls `Delete()`, saves

### Application — DTOs (1 new file)
- `DTOs/UpdateStatusRequest.cs` — payload for PUT endpoint (`NewStatus` + `Notes`)

### Domain (1 method added)
- `JobApplication.cs` — added `UpdateStatus(ApplicationStatus newStatus, string? notes)` method

### Domain — Interfaces (1 method added)
- `IJobApplicationRepository.cs` — added `void Delete(JobApplication application)`

### Infrastructure (1 method added)
- `Persistence/Repositories/JobApplicationRepository.cs` — implemented `Delete()` via `_context.JobApplications.Remove()`

### Api (2 endpoints added)
- `Controllers/JobApplicationsController.cs` — `PUT /{id}` + `DELETE /{id}`

---

## Final API surface

| Method | Route | Returns | Description |
|--------|-------|---------|-------------|
| `GET` | `/api/jobapplications/{userId}` | `200 OK` + `[]` | List all applications for a user |
| `POST` | `/api/jobapplications` | `201 Created` + `{ id }` | Create a new application |
| `PUT` | `/api/jobapplications/{id}` | `204 No Content` | Update application status |
| `DELETE` | `/api/jobapplications/{id}` | `204 No Content` | Delete an application |

---

## Bugs fixed

### Bug 1 — POST returned 201 but never saved to PostgreSQL

**Symptom:** `POST` returned `201 Created` with a valid Guid.
Verified with `SELECT "Id" FROM "JobApplications"` → `0 rows`.

**Root cause:** `CreateJobApplicationHandler` was missing `SaveChangesAsync`.
The entities were added to the EF Core change tracker but never committed.

```csharp
// ❌ Before — data added to context but never persisted
await _jobApplicationRepository.AddAsync(application, cancellationToken);
return application.Id;

// ✅ After — SaveChangesAsync commits to PostgreSQL
await _jobApplicationRepository.AddAsync(application, cancellationToken);
await _unitOfWork.SaveChangesAsync(cancellationToken); // ← this line was missing
return application.Id;
```

**Lesson:** EF Core's `Add()` / `AddAsync()` only tracks the entity in memory.
Nothing hits the database until `SaveChangesAsync()` is called.

---

### Bug 2 — PUT returned 404 even though POST returned 201

**Symptom:** `PUT /api/jobapplications/{id}` → `404 Not Found`.
The Guid came directly from the POST response.

**Root cause:** Bug 1 above — the record was never in the database.
`GetByIdAsync` returned `null` because the table was empty.

**Fix:** Fixing `SaveChangesAsync` in Bug 1 resolved this automatically.

---

### Bug 3 — Foreign Key violation on POST after fixing Bug 1

**Symptom:**
```
DbUpdateException: insert or update on table "JobApplications" violates
foreign key constraint "FK_JobApplications_Users_UserId"
```

**Root cause:** The `Users` table was empty. PostgreSQL enforced the FK constraint
and rejected the insert because `UserId = '11111111-...'` didn't exist.

**Fix (temporary — until JWT auth is implemented in Bloque 11):**
Insert a seed user directly in PostgreSQL:

```sql
INSERT INTO "Users" ("Id", "FullName", "Email", "PasswordHash", "CreatedAt")
VALUES (
  '11111111-1111-1111-1111-111111111111',
  'Ramiro López',
  'ramiro@test.com',
  'hash-placeholder',
  NOW()
);
```

**Note:** This is a test workaround. Once JWT auth is implemented,
users will be created through `POST /api/auth/register`.

---

### Bug 4 — Build failed: DLLs locked by running process

**Symptom:**
```
error MSB3027: Could not copy JobTrackerPro.Application.dll
The file is locked by: "JobTrackerPro.Api (19528)"
```

**Root cause:** Tried to run `dotnet build` while the API was still running.
The process held locks on the DLL files.

**Fix:** `Ctrl + C` to stop the API, then rebuild.

**Rule:** Always stop the API before rebuilding.

---

### Bug 5 — NuGet version conflict (MSB3277 warning)

**Symptom:**
```
warning MSB3277: Found conflicts between Microsoft.EntityFrameworkCore.Relational
Version 10.0.0 vs 10.0.3
```

**Attempted fix (wrong):** Changed `Npgsql.EntityFrameworkCore.PostgreSQL` to `10.0.3` —
that version does not exist on NuGet. Caused `NU1103` restore errors.

**Correct fix:** Reverted to original versions. The MSB3277 warning is **inofensivo**
(build succeeds, app runs correctly). This was premature optimization.

**Lesson:** A warning that doesn't break the build can wait.
Don't fix warnings by blindly changing version numbers without verifying they exist.

---

## Verified CRUD flow — Swagger end-to-end

```
1. POST  /api/JobApplications
         body: { userId, title, companyName, jobUrl, description, source }
         → 201 Created { "id": "8ae2c77d-..." } ✅

2. PUT   /api/JobApplications/8ae2c77d-...
         body: { "newStatus": 1, "notes": "Phone screen scheduled" }
         → 204 No Content ✅

3. GET   /api/JobApplications/11111111-...
         → 200 OK [ { status: 1, ... } ] ✅  (status updated)

4. DELETE /api/JobApplications/8ae2c77d-...
         → 204 No Content ✅

5. GET   /api/JobApplications/11111111-...
         → 200 OK [] ✅  (empty — record deleted)
```

---

## Architecture in action

```
PUT /api/JobApplications/{id}
    ↓
JobApplicationsController
    ↓ ISender.Send(UpdateJobApplicationCommand(id, newStatus, notes))
MediatR
    ↓
UpdateJobApplicationHandler
    ↓ IJobApplicationRepository.GetByIdAsync(id)  → finds entity
    ↓ application.UpdateStatus(newStatus, notes)   → mutates domain entity
    ↓ IUnitOfWork.SaveChangesAsync()               → commits to PostgreSQL
    ↓ returns true
Controller
    ↓ NoContent() → 204
```

---

## Key concept: Unit of Work

All operations in a single request share **one transaction**:

```csharp
// One SaveChangesAsync = one atomic DB transaction
await _repository.AddAsync(application, cancellationToken);
await _unitOfWork.SaveChangesAsync(cancellationToken); // ← single commit
```

If `SaveChangesAsync` is not called → changes exist in memory but **never reach PostgreSQL**.
This was exactly Bug 1.

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
| **9** | **CRUD Completo: PUT + DELETE** | ✅ |
| 10 | FluentValidation + Global Error Handler | ⏳ next |

---

## Commits

```
feat: wire up full CRUD with PUT and DELETE endpoints
```
