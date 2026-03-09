# Day 05 — API Controller + Swagger UI

**Date:** March 8, 2026
**Phase:** 1 — Backend Core
**Block:** Bloque 6 — First Controller + Swagger
**Duration:** ~1 hour

---

## What I did

Created the first real API controller and configured Swagger UI.
The full request pipeline is now connected end to end.

---

## Files created

### Api (2 files)
- `Controllers/JobApplicationsController.cs` — GET + POST endpoints
- `Program.cs` — updated with Swagger + Controllers

### NuGet packages added
- `Swashbuckle.AspNetCore` v10.1.4 → Api

---

## Full request pipeline (now testable)

```
Browser / Swagger UI
     ↓ POST /api/jobapplications
JobApplicationsController
     ↓ ISender.Send(CreateJobApplicationCommand)
MediatR
     ↓
CreateJobApplicationHandler
     ↓ ICompanyRepository.GetByNameAsync()
     ↓ ICompanyRepository.AddAsync()         → ApplicationDbContext → PostgreSQL
     ↓ IJobApplicationRepository.AddAsync()  → ApplicationDbContext → PostgreSQL
     ↓ returns Guid
Controller
     ↓ 201 Created { id: "..." }
Browser
```

---

## Controller design decisions

**Why ISender instead of IMediator?**
`ISender` is a smaller interface — only exposes `Send()`.
`IMediator` also exposes `Publish()` for domain events.
Controllers only need to send commands/queries, so `ISender` is cleaner.

**Why no [Authorize] yet?**
JWT auth comes in Week 3. For now endpoints are open to test the pipeline.

**Why CreatedAtAction?**
REST convention: POST returns 201 Created with a Location header
pointing to where the resource can be retrieved.

---

## Swagger UI

Available at `https://localhost:xxxx/` (root) in Development mode.

Endpoints visible in Swagger:
- `GET  /api/jobapplications/{userId}` — returns all applications for a user
- `POST /api/jobapplications` — creates a new application

---

## What still needs PostgreSQL running

Calling the endpoints will fail at runtime until:
1. PostgreSQL is running (Docker — next block)
2. EF Core migrations are applied (`dotnet ef database update`)

The app **compiles and starts** without PostgreSQL.
It only fails when a request actually tries to query the DB.

---

## Build result

```
Build succeeded in 4.6s — 6/6 projects compiled ✓
0 errors · 0 warnings
```

---

## Phase 1 progress

| Block | Content | Status |
|-------|---------|--------|
| 1 | Clean Architecture setup | ✅ |
| 2-3 | Domain: Entities + Interfaces | ✅ |
| 4 | Application: Commands + Queries + DTOs | ✅ |
| 5 | Infrastructure: Repositories + DI | ✅ |
| 6 | API: Controller + Swagger | ✅ |
| **7** | **EF Core Migrations + PostgreSQL (Docker)** | ⏳ next |

---

## Commit

```
feat: add JobApplicationsController and Swagger UI
```
