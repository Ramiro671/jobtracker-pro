# Day 03 — Application Layer: MediatR + DTOs + Handlers

**Date:** March 8, 2026
**Phase:** 1 — Backend Core
**Block:** Bloque 4 — Application Layer: Use Cases + DTOs
**Duration:** ~1 hour

---

## What I did

Built the Application layer from scratch using MediatR and the CQRS pattern.
Also fixed a missing NuGet package in Infrastructure (EF Core was referenced but not installed).

---

## Files created

### DTOs (1)
- `DTOs/JobApplicationDto.cs` — record with all fields returned to the client

### Commands (2)
- `JobApplications/Commands/CreateJobApplicationCommand.cs` — IRequest<Guid>
- `JobApplications/Commands/CreateJobApplicationHandler.cs` — creates company if not exists, then creates application

### Queries (2)
- `JobApplications/Queries/GetJobApplicationsQuery.cs` — IRequest<IReadOnlyList<JobApplicationDto>>
- `JobApplications/Queries/GetJobApplicationsHandler.cs` — maps domain entities to DTOs

### DI Registration (1)
- `Common/DependencyInjection.cs` — registers MediatR from this assembly

---

## Bug fixed

`ApplicationDbContext.cs` already existed in Infrastructure but EF Core packages were missing.

```
Error: CS0234 — 'EntityFrameworkCore' does not exist in namespace 'Microsoft'
Fix:
  dotnet add src/JobTrackerPro.Infrastructure package Microsoft.EntityFrameworkCore
  dotnet add src/JobTrackerPro.Infrastructure package Npgsql.EntityFrameworkCore.PostgreSQL
```

**Lesson:** The .csproj file must explicitly declare every NuGet package. Having a `.cs` file with `using Microsoft.EntityFrameworkCore` is not enough — the package must be installed first.

---

## Architecture in action

```
HTTP Request
    ↓
JobApplicationsController (Api)
    ↓  sends CreateJobApplicationCommand
MediatR dispatcher
    ↓  routes to handler
CreateJobApplicationHandler (Application)
    ↓  calls ICompanyRepository + IJobApplicationRepository
    ↓  (interfaces defined in Domain)
[Implementation lives in Infrastructure — not wired yet]
```

Application does NOT know about:
- HTTP (no HttpContext, no IActionResult)
- EF Core (no DbContext, no DbSet)
- PostgreSQL

It only knows about Domain interfaces. That's the point.

---

## CQRS pattern

| Type | Class | Returns | Changes state? |
|------|-------|---------|----------------|
| Command | `CreateJobApplicationCommand` | `Guid` (new Id) | ✅ Yes |
| Query | `GetJobApplicationsQuery` | `IReadOnlyList<JobApplicationDto>` | ❌ No |

**Rule:** Commands change state, Queries read state. Never mix both.

---

## NuGet packages added today

| Package | Project | Why |
|---------|---------|-----|
| `MediatR` | Application | CQRS dispatcher |
| `AutoMapper` | Application | Entity → DTO mapping (next block) |
| `MediatR` | Api | ISender injection in controllers |
| `AutoMapper` | Api | DI registration |
| `Microsoft.EntityFrameworkCore` | Infrastructure | DbContext, DbSet, ModelBuilder |
| `Npgsql.EntityFrameworkCore.PostgreSQL` | Infrastructure | PostgreSQL provider |

---

## Build result

```
Build succeeded in 5.1s — 6/6 projects compiled ✓
0 errors · 0 warnings
```

---

## What's missing (next block)

Application layer handlers use `IJobApplicationRepository` and `ICompanyRepository` —
but Infrastructure has no implementations yet. Calling the API would throw at runtime.

**Next:** Bloque 5 — Implement repositories in Infrastructure + wire up DI in Program.cs.

---

## Commit

```
feat: add application layer with MediatR commands, queries, handlers and DTOs
fix: add missing EF Core NuGet packages to Infrastructure
```
