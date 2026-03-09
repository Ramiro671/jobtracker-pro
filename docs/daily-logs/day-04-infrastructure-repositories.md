# Day 04 — Infrastructure: Repositories + Unit of Work + DI Wiring

**Date:** March 8, 2026
**Phase:** 1 — Backend Core
**Block:** Bloque 5 — Infrastructure Repositories + Program.cs
**Duration:** ~1 hour

---

## What I did

Implemented the full Infrastructure layer: 3 repositories, Unit of Work,
DI registration, and wired everything together in Program.cs.

---

## Files created

### Domain (1 new interface)
- `Interfaces/IUnitOfWork.cs` — contract for committing DB changes

### Infrastructure (5 new files)
- `Persistence/UnitOfWork.cs` — wraps SaveChangesAsync
- `Persistence/Repositories/JobApplicationRepository.cs` — EF Core impl
- `Persistence/Repositories/CompanyRepository.cs` — EF Core impl
- `Persistence/Repositories/UserRepository.cs` — EF Core impl
- `DependencyInjection.cs` — registers DbContext + repos + UoW

### Api (1 updated)
- `Program.cs` — replaced WeatherForecast stub with real DI wiring

### Config (1 updated)
- `appsettings.json` — added ConnectionStrings:DefaultConnection

---

## Full dependency flow (now complete)

```
HTTP Request
     ↓
Program.cs → builder.Services.AddApplication()
           → builder.Services.AddInfrastructure()
     ↓
JobApplicationsController
     ↓ ISender.Send(CreateJobApplicationCommand)
MediatR
     ↓ routes to handler
CreateJobApplicationHandler (Application)
     ↓ ICompanyRepository → CompanyRepository (Infrastructure)
     ↓ IJobApplicationRepository → JobApplicationRepository (Infrastructure)
     ↓ both use ApplicationDbContext → PostgreSQL
```

---

## What DI does here

```csharp
// Program.cs registers:
services.AddScoped<IJobApplicationRepository, JobApplicationRepository>();

// Handler declares:
public CreateJobApplicationHandler(IJobApplicationRepository repo) { }

// .NET resolves at runtime:
// "You asked for IJobApplicationRepository? Here's a JobApplicationRepository."
```

The handler never imports `JobApplicationRepository` directly.
It only knows the interface. Infrastructure could be swapped without touching Application.

---

## Unit of Work pattern

Without UoW, each repository calls `SaveChanges` independently.
With UoW, multiple operations share one transaction:

```csharp
// Create company + create application = ONE SaveChanges call
await _companyRepository.AddAsync(company);
await _jobApplicationRepository.AddAsync(application);
await _unitOfWork.SaveChangesAsync(); // commits both atomically
```

---

## Build result

```
Build succeeded in 3.1s — 6/6 projects compiled ✓
0 errors · 0 warnings
```

---

## What's NOT done yet

- No EF Core migrations (need PostgreSQL running — next: Docker)
- No Controller for JobApplications (next block)
- No JWT authentication (coming in Phase 1 week 3)

---

## Next block

Bloque 6 — First API Controller (JobApplicationsController) + Swagger
So we can test the full flow: HTTP → Controller → MediatR → Handler → Repository

---

## Commits

```
feat: implement repositories, unit of work and wire DI in Program.cs
```
