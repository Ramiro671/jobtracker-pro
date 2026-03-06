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

## Glossary — Key Concepts (Day 01)

---

### What is a "dependency"?

A **dependency** is anything a class needs from outside itself to do its job.

```csharp
// CreateJobOfferHandler DEPENDS ON IJobOfferRepository
// It cannot work without it — it's a dependency
public class CreateJobOfferHandler
{
    private readonly IJobOfferRepository _repo; // ← dependency

    // The dependency arrives FROM OUTSIDE (injected), not created inside
    public CreateJobOfferHandler(IJobOfferRepository repo) => _repo = repo;
}
```

Without `IJobOfferRepository`, the handler cannot save a job offer. That's a dependency.
The key is: the handler asks for an **interface**, not a specific class.
Who provides the real implementation? The DI container in `Program.cs`.

---

### Glossary

| Term | Layer | Description | Code Example |
|------|-------|-------------|--------------|
| **ASP.NET Core Web API** | Api | Framework de Microsoft para construir APIs HTTP en C#. Recibe requests (GET/POST/PUT/DELETE) y devuelve JSON. Punto de entrada de la app. | `[ApiController] public class JobOffersController : ControllerBase { }` |
| **Middleware** | Api | Código que se ejecuta en **cada request**, antes y después del controller. Se encadenan como tubería. Ej: auth, logging, errores globales. | `app.UseMiddleware<ExceptionHandlingMiddleware>();` |
| **Dependency Injection** | Api | Las dependencias llegan **desde afuera** inyectadas por el framework. La clase pide una interfaz, el contenedor provee la implementación real. | `public Controller(IJobOfferRepository repo) => _repo = repo;` |
| **Value Object** | Domain | Objeto definido por su **valor**, no por un ID. Sin identidad propia. Inmutable. Dos `Email` con el mismo valor son iguales. | `public record Email(string Value);` |
| **Entity** | Domain | Objeto con **identidad propia** (Id único). Dos `JobOffer` con mismo título pero distinto Id son diferentes. | `public class JobOffer { public Guid Id { get; } }` |
| **Interface** | Domain | Contrato que define QUÉ métodos existen, sin decir CÓMO se implementan. Domain los define, Infrastructure los implementa. | `public interface IJobOfferRepository { Task<IEnumerable<JobOffer>> GetAllAsync(); }` |
| **MediatR** | Application | Librería que implementa el patrón Mediator. Desacopla quien envía una acción de quien la ejecuta. El controller envía, el handler ejecuta. | `await _sender.Send(new CreateJobOfferCommand(...));` |
| **Command** | Application | Mensaje que representa una **acción** que cambia estado. Se envía via MediatR. Tiene un Handler que lo procesa. | `public record CreateJobOfferCommand(string Title) : IRequest<Guid>;` |
| **Query** | Application | Mensaje que **consulta datos** sin cambiar estado. También tiene un Handler. | `public record GetJobOffersQuery : IRequest<List<JobOfferDto>>;` |
| **Handler** | Application | Clase que **ejecuta** un Command o Query. Contiene la lógica del caso de uso. Un handler por acción. | `public class CreateJobOfferHandler : IRequestHandler<CreateJobOfferCommand, Guid> { }` |
| **DTO** | Application | Data Transfer Object — objeto simple para mover datos entre capas. Sin lógica. Lo que la API devuelve al cliente, no la entidad completa. | `public record JobOfferDto(Guid Id, string Title, string Status);` |
| **EF Core** | Infrastructure | ORM de Microsoft. Traduce clases C# a tablas SQL. Gestiona migraciones y consultas LINQ → SQL. | `await _db.JobOffers.Where(j => j.Status == Applied).ToListAsync();` |
| **Repository** | Infrastructure | Implementa las interfaces del Domain. Abstrae el acceso a datos. La app no sabe si los datos vienen de PostgreSQL o MongoDB. | `public class JobOfferRepository : IJobOfferRepository { ... }` |
| **JWT** | Infrastructure | Token firmado para autenticación stateless. El servidor lo genera en login, el cliente lo envía en cada request. Sin consultar la DB. | `services.AddAuthentication().AddJwtBearer(opt => { opt.TokenValidationParameters = ...; });` |
| **Redis** | Infrastructure | Base de datos en memoria ultrarrápida. Caché para no consultar PostgreSQL en cada request. | `await _cache.SetStringAsync("jobs", json, new DistributedCacheEntryOptions { AbsoluteExpiration = ... });` |
| **Unit Test** | Tests | Prueba **una sola clase en aislamiento**. Sin DB, sin red. Usa Mocks para simular dependencias. Muy rápido. | `var mock = new Mock<IJobOfferRepository>(); mock.Setup(r => r.GetAllAsync()).ReturnsAsync(fakeList);` |
| **Integration Test** | Tests | Prueba **múltiples capas juntas**. Levanta la app completa en memoria con `WebApplicationFactory`. Más lento, más realista. | `var client = _factory.CreateClient(); var response = await client.GetAsync("/api/job-offers");` |

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
