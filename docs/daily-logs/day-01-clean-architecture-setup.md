# Day 01 — Clean Architecture Setup

**Date:** March 5, 2026  
**Phase:** 1 — Backend Core  
**Block:** Bloque 1 — Setup Clean Architecture  
**Duration:** ~1 hour

---

## What I did

Set up the complete Clean Architecture solution structure for JobTracker Pro.

### Solution structure created

```
JobTrackerPro.sln
├── src/
│   ├── JobTrackerPro.Api/            # ASP.NET Core Web API — controllers, middleware, DI
│   ├── JobTrackerPro.Domain/         # Entities, interfaces, value objects
│   ├── JobTrackerPro.Application/    # Use cases, DTOs, MediatR handlers
│   └── JobTrackerPro.Infrastructure/ # EF Core, repositories, JWT, Redis
└── tests/
    ├── JobTrackerPro.UnitTests/
    └── JobTrackerPro.IntegrationTests/
```

### Project references configured

| Project | References |
|---|---|
| Api | Application + Infrastructure |
| Application | Domain only |
| Infrastructure | Domain + Application |
| UnitTests | Application + Domain |
| IntegrationTests | Api |

### Commands used

```bash
dotnet new sln -n JobTrackerPro
dotnet new webapi -n JobTrackerPro.Api -o src/JobTrackerPro.Api
dotnet new classlib -n JobTrackerPro.Domain -o src/JobTrackerPro.Domain
dotnet new classlib -n JobTrackerPro.Application -o src/JobTrackerPro.Application
dotnet new classlib -n JobTrackerPro.Infrastructure -o src/JobTrackerPro.Infrastructure
dotnet new xunit -n JobTrackerPro.UnitTests -o tests/JobTrackerPro.UnitTests
dotnet new xunit -n JobTrackerPro.IntegrationTests -o tests/JobTrackerPro.IntegrationTests
dotnet build
```

### Result

```
Build succeeded in 14.6s — 6/6 projects compiled ✓
```

---

## What I learned

**Clean Architecture** separates concerns into layers with strict dependency rules:

- **Domain** — the core. No dependencies. Defines entities and interfaces.
- **Application** — orchestrates use cases. Only knows Domain.
- **Infrastructure** — implements interfaces (EF Core, repositories, JWT). Knows Domain + Application.
- **Api** — entry point. Wires everything together via DI.

**Key rule:** Dependencies always point **inward** toward Domain. Domain never depends on anything external.

---

## Key concept: Dependency Inversion

Domain defines the **contract** (interface), Infrastructure provides the **implementation**.

```csharp
// Domain — defines the contract
public interface IJobOfferRepository
{
    Task<IEnumerable<JobOffer>> GetAllAsync();
}

// Infrastructure — provides the implementation
public class JobOfferRepository : IJobOfferRepository { ... }
```

This means I can swap PostgreSQL for any database without touching Domain or Application.

---

## Why this matters for interviews

When asked *"What is Clean Architecture?"* I can now answer:

> "It's a layered architecture where dependencies always point inward toward the Domain. The Domain has no external dependencies — it only defines entities and interfaces. The Application layer orchestrates use cases. Infrastructure implements those interfaces. The API wires everything together through dependency injection."

---

## Tomorrow — Bloque 2

Create Domain entities:
- `JobOffer.cs` · `Company.cs` · `User.cs`
- `JobStatus` enum (Applied / Interview / Rejected / Offer)
- Repository interfaces: `IJobOfferRepository` · `IUserRepository`

---

## Commit

```
feat: add clean architecture solution structure with 4 layers and tests
```
