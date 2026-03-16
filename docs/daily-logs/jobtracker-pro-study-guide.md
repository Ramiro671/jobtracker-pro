# JobTracker Pro — Guía de Estudio Completa / Complete Study Guide
### Full-Stack Interview Preparation | Preparación para Entrevistas
**Stack:** .NET 10 · React 18 · PostgreSQL · Redis · Azure · GitHub Actions

---

# SECTION 1: CORE WEB CONCEPTS / Conceptos Web Fundamentales

---

## 1.1 What is an API / REST API — ¿Qué es una API / REST API?

**[EN]** An API (Application Programming Interface) is a contract that defines how two software systems communicate. A REST API (Representational State Transfer) is an API that follows 6 architectural constraints: stateless communication, uniform interface, client-server separation, cacheability, layered system, and optional code-on-demand. Resources are identified by URLs and manipulated via HTTP methods.

**[ES]** Una API es un contrato que define cómo dos sistemas de software se comunican. Una REST API sigue 6 restricciones arquitectónicas: comunicación sin estado, interfaz uniforme, separación cliente-servidor, capacidad de caché, sistema en capas y código bajo demanda opcional. Los recursos se identifican con URLs y se manipulan con métodos HTTP.

**How it works / Cómo funciona:**
```
Client → HTTP Request (GET /api/jobapplications) → Server
Server → Processes request → Returns JSON Response
Client ← HTTP Response (200 OK + JSON body) ←
```

**In JobTracker Pro:** `GET /api/jobapplications` returns all job applications for the authenticated user as a JSON array.

---

## 1.2 HTTP Methods and Status Codes — Métodos y Códigos de Estado HTTP

### HTTP Methods / Métodos HTTP

| Method | Purpose [EN] | Propósito [ES] | Idempotent |
|--------|-------------|----------------|------------|
| GET | Read a resource | Leer un recurso | ✅ Yes |
| POST | Create a resource | Crear un recurso | ❌ No |
| PUT | Replace a resource entirely | Reemplazar recurso completo | ✅ Yes |
| PATCH | Partially update a resource | Actualizar parcialmente | ❌ No |
| DELETE | Remove a resource | Eliminar un recurso | ✅ Yes |

### Status Codes / Códigos de Estado

| Code | Meaning [EN] | Significado [ES] | JobTracker Usage |
|------|-------------|------------------|-----------------|
| 200 | OK | Exitoso | GET returns data |
| 201 | Created | Creado | POST new application |
| 204 | No Content | Sin contenido | DELETE, PUT success |
| 400 | Bad Request | Solicitud inválida | Validation errors |
| 401 | Unauthorized | No autenticado | Missing/invalid JWT |
| 403 | Forbidden | Sin permiso | Valid JWT, wrong user |
| 404 | Not Found | No encontrado | Application doesn't exist |
| 422 | Unprocessable Entity | Entidad no procesable | FluentValidation errors |
| 429 | Too Many Requests | Demasiadas solicitudes | Rate limit exceeded |
| 500 | Internal Server Error | Error del servidor | Unhandled exception |

---

## 1.3 What is JSON — ¿Qué es JSON?

**[EN]** JSON (JavaScript Object Notation) is a lightweight, text-based data format for exchanging data between systems. It is language-independent, human-readable, and natively supported in JavaScript.

**[ES]** JSON es un formato de texto ligero para intercambiar datos entre sistemas. Es independiente del lenguaje, legible por humanos y soportado nativamente en JavaScript.

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "company": "Microsoft",
  "position": "Senior .NET Developer",
  "status": 1,
  "appliedAt": "2026-03-13T10:00:00Z",
  "tags": ["remote", "senior"],
  "salary": null
}
```

**Types / Tipos:** string, number, boolean, null, object `{}`, array `[]`

---

## 1.4 Web Server vs Client — Servidor Web vs Cliente

**[EN]** A **client** (browser, mobile app) initiates requests. A **server** (ASP.NET Core + Kestrel) listens for requests, processes business logic, accesses the database, and returns responses. They communicate over HTTP/HTTPS.

**[ES]** El **cliente** (navegador, app móvil) inicia las peticiones. El **servidor** (ASP.NET Core + Kestrel) escucha, procesa la lógica de negocio, accede a la base de datos y retorna respuestas.

**In JobTracker Pro:**
- Client: React app on GitHub Pages (ramiro671.github.io/jobtracker-pro)
- Server: ASP.NET Core 10 on Azure App Service

---

## 1.5 HTTPS / TLS — HTTPS / TLS

**[EN]** HTTPS = HTTP + TLS (Transport Layer Security). TLS encrypts all data in transit using asymmetric encryption (handshake) then symmetric encryption (session). This prevents man-in-the-middle attacks and eavesdropping. Required when transmitting JWTs.

**[ES]** HTTPS = HTTP + TLS. TLS cifra todos los datos en tránsito usando cifrado asimétrico (handshake) luego simétrico (sesión). Previene ataques de intermediario. Obligatorio al transmitir JWTs.

---

## 1.6 CORS — Cross-Origin Resource Sharing

**[EN]** CORS is a browser security mechanism that blocks requests from a different origin (domain + port + protocol) unless the server explicitly allows it. The browser sends a `preflight` OPTIONS request first.

**[ES]** CORS es un mecanismo de seguridad del navegador que bloquea peticiones de un origen diferente (dominio + puerto + protocolo) a menos que el servidor lo permita explícitamente.

**In JobTracker Pro (`Program.cs`):**
```csharp
app.UseCors(policy => policy
    .WithOrigins(
        "http://localhost:5173",
        "https://ramiro671.github.io"
    )
    .AllowAnyMethod()
    .AllowAnyHeader()
    .AllowCredentials());
```

**Why it exists / Por qué existe:** Prevents malicioussite.com from making authenticated requests to yourbank.com using your cookies.

---

## 1.7 Cookie vs localStorage vs sessionStorage

| Feature | Cookie | localStorage | sessionStorage |
|---------|--------|--------------|----------------|
| Expiry | Configurable | Never (manual) | Tab close |
| Accessible via JS | Yes (unless httpOnly) | Yes | Yes |
| Sent with requests | Automatically | No (manual) | No |
| Size | ~4KB | ~5-10MB | ~5-10MB |
| XSS vulnerable | If not httpOnly | ✅ Yes | ✅ Yes |
| CSRF vulnerable | ✅ Yes | ❌ No | ❌ No |

**In JobTracker Pro:** JWT stored in `localStorage` (keys: `accessToken`, `userId`). Tradeoff: vulnerable to XSS but no CSRF risk.

---

## 1.8 SPA vs MPA — Single Page Application vs Multi-Page Application

**[EN]** A **SPA** loads one HTML file and dynamically updates the DOM via JavaScript. Navigation feels instant (no full page reload). A **MPA** serves a new HTML page from the server on every navigation.

**[ES]** Una **SPA** carga un HTML y actualiza el DOM dinámicamente. La navegación es instantánea. Una **MPA** sirve un nuevo HTML desde el servidor en cada navegación.

**In JobTracker Pro:** React SPA hosted on GitHub Pages. SPA routing is handled by `frontend/public/404.html` — GitHub Pages serves it on unknown paths, which redirects back to `index.html`. `BrowserRouter` uses `basename="/jobtracker-pro"`.

---

## 1.9 Build Tools — Herramientas de Construcción (Vite)

**[EN]** A build tool transforms source code (TypeScript, JSX, CSS modules) into optimized browser-ready assets (JavaScript bundles, minified CSS). Vite uses native ES modules during development (instant HMR) and Rollup for production builds.

**[ES]** Una herramienta de construcción transforma código fuente (TypeScript, JSX) en assets optimizados para el navegador. Vite usa módulos ES nativos en desarrollo (HMR instantáneo) y Rollup para producción.

**In JobTracker Pro:** `npm run build` → generates `frontend/dist/` → deployed to GitHub Pages.

---

# SECTION 2: BACKEND — .NET AND C# / Backend con .NET y C#

---

## 2.1 What is .NET and the CLR

**[EN]** .NET is a cross-platform framework for building applications. The CLR (Common Language Runtime) is the virtual machine that executes .NET code. It handles memory management (garbage collection), JIT compilation (converts IL to native code at runtime), and type safety.

**[ES]** .NET es un framework multiplataforma para construir aplicaciones. El CLR es la máquina virtual que ejecuta código .NET. Maneja memoria (garbage collection), compilación JIT y seguridad de tipos.

---

## 2.2 ASP.NET Core — Kestrel and Middleware Pipeline

**[EN]** ASP.NET Core is a web framework built on .NET. **Kestrel** is the default cross-platform web server embedded in the application. The **middleware pipeline** is a chain of components that process requests and responses in order.

**[ES]** ASP.NET Core es un framework web. **Kestrel** es el servidor web multiplataforma embebido. El **pipeline de middleware** es una cadena de componentes que procesan peticiones y respuestas en orden.

```
Request → [HTTPS Redirection] → [CORS] → [Rate Limiting] → [Auth] → [Routing] → Controller
Response ← [Exception Handler] ← ... ← [Auth] ← [Routing] ← Controller
```

**Order matters! / ¡El orden importa!** Authentication must come before Authorization. CORS must come before routing.

---

## 2.3 Dependency Injection and IoC Container

**[EN]** DI is a technique where a class receives its dependencies from external code instead of creating them itself. The IoC (Inversion of Control) Container manages object creation and lifetime. Lifetimes: **Singleton** (one instance), **Scoped** (one per request), **Transient** (new each time).

**[ES]** DI es una técnica donde una clase recibe sus dependencias del exterior. El contenedor IoC gestiona la creación y el ciclo de vida de los objetos.

```csharp
// Registration (Program.cs)
builder.Services.AddScoped<IJobApplicationRepository, JobApplicationRepository>();
builder.Services.AddScoped<ICacheService, RedisCacheService>();

// Consumption (Constructor Injection)
public class GetJobApplicationsHandler(IJobApplicationRepository repository)
{
    // repository injected automatically
}
```

---

## 2.4 Controllers and Routing

**[EN]** A controller is a class that handles HTTP requests. Routing maps a URL + method to a controller action.

**[ES]** Un controller es una clase que maneja peticiones HTTP. El routing mapea una URL + método a una acción del controller.

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class JobApplicationsController : ControllerBase
{
    [HttpGet]                    // GET /api/jobapplications
    [HttpGet("{id}")]            // GET /api/jobapplications/{id}
    [HttpPost]                   // POST /api/jobapplications
    [HttpPut("{id}")]            // PUT /api/jobapplications/{id}
    [HttpDelete("{id}")]         // DELETE /api/jobapplications/{id}
}
```

---

## 2.5 async/await and Task

**[EN]** `async/await` allows non-blocking I/O operations. When a method `await`s an async operation (DB call, HTTP request), the thread is released to handle other requests instead of blocking. This improves scalability.

**[ES]** `async/await` permite operaciones de I/O no bloqueantes. Cuando un método hace `await`, el hilo se libera para manejar otras peticiones en lugar de bloquear.

```csharp
// ❌ Blocking — wastes thread
var result = repository.GetAllAsync().Result;

// ✅ Non-blocking — thread is free while DB query runs
var result = await repository.GetAllAsync();
```

---

## 2.6 LINQ

**[EN]** LINQ (Language Integrated Query) allows querying collections using C# syntax. Translates to SQL when used with EF Core.

**[ES]** LINQ permite consultar colecciones usando sintaxis C#. Se traduce a SQL cuando se usa con EF Core.

```csharp
var applications = await dbContext.JobApplications
    .Where(a => a.UserId == userId && a.Status == ApplicationStatus.Applied)
    .OrderByDescending(a => a.CreatedAt)
    .Select(a => new JobApplicationDto { Id = a.Id, Company = a.Company })
    .ToListAsync();
```

---

## 2.7 Interfaces in C#

**[EN]** An interface defines a contract (method signatures) without implementation. Using interfaces enables: dependency injection, unit testing with mocks, swapping implementations without changing dependent code.

**[ES]** Una interfaz define un contrato sin implementación. Permite: inyección de dependencias, pruebas unitarias con mocks, y cambiar implementaciones sin afectar el código dependiente.

```csharp
public interface IJobApplicationRepository
{
    Task<IEnumerable<JobApplication>> GetAllByUserIdAsync(Guid userId);
    Task<JobApplication?> GetByIdAsync(Guid id);
    Task AddAsync(JobApplication application);
    Task UpdateAsync(JobApplication application);
    Task DeleteAsync(Guid id);
}
```

---

## 2.8 C# Records vs Classes vs Structs

| Feature | Record | Class | Struct |
|---------|--------|-------|--------|
| Reference type | ✅ | ✅ | ❌ (value type) |
| Immutable by default | ✅ | ❌ | ❌ |
| Value equality | ✅ | ❌ (reference) | ✅ |
| Use case | DTOs, Commands | Domain entities | Small value types |

**In JobTracker Pro:** Commands and Queries are records:
```csharp
public record CreateJobApplicationCommand(
    string Company,
    string Position,
    ApplicationStatus Status
) : IRequest<JobApplicationDto>;
```

---

# SECTION 3: CLEAN ARCHITECTURE / Arquitectura Limpia

---

## 3.1 What is Clean Architecture

**[EN]** Clean Architecture (Robert C. Martin) organizes code in concentric layers where dependencies flow **inward only** — outer layers know about inner layers, but inner layers know nothing about outer layers. This makes the core business logic independent of frameworks, databases, and UI.

**[ES]** Clean Architecture organiza el código en capas concéntricas donde las dependencias fluyen **solo hacia adentro**. Esto hace que la lógica de negocio sea independiente de frameworks, bases de datos y UI.

```
┌─────────────────────────────────────┐
│           API Layer                 │  ← Depends on: Application, Infrastructure
│  Controllers, Middleware, Program   │
├─────────────────────────────────────┤
│       Infrastructure Layer          │  ← Depends on: Application, Domain
│  EF Core, Repos, JWT, Redis, SMTP   │
├─────────────────────────────────────┤
│        Application Layer            │  ← Depends on: Domain ONLY
│  MediatR Handlers, DTOs, Validators │
├─────────────────────────────────────┤
│          Domain Layer               │  ← Depends on: NOTHING
│  Entities, Interfaces, Enums, VOs   │
└─────────────────────────────────────┘
         ↑ Dependencies point INWARD ↑
```

---

## 3.2 The 4 Layers — Las 4 Capas

### Domain Layer (innermost)
- **Contains / Contiene:** Entities (`JobApplication`, `User`), Enums (`ApplicationStatus`), Interfaces (`IJobApplicationRepository`), Value Objects
- **Rule / Regla:** Zero external NuGet dependencies. Pure C#.

### Application Layer
- **Contains / Contiene:** MediatR Commands/Queries/Handlers, DTOs, FluentValidation validators, Pipeline Behaviors
- **Rule / Regla:** Only depends on Domain. No knowledge of EF Core, HTTP, or databases.

### Infrastructure Layer
- **Contains / Contiene:** EF Core `AppDbContext`, Repository implementations, JWT service, Redis cache service, SMTP email service, Migrations
- **Rule / Regla:** Implements interfaces defined in Domain/Application.

### API Layer (outermost)
- **Contains / Contiene:** Controllers, Middleware (exception handler, rate limiter), `Program.cs` (DI registration, pipeline configuration)
- **Rule / Regla:** Entry point. Coordinates but contains no business logic.

---

## 3.3 The Dependency Inversion Principle (SOLID - D)

**[EN]** High-level modules should not depend on low-level modules. Both should depend on abstractions. In practice: Domain defines `IJobApplicationRepository`. Infrastructure implements it. Application uses it via the interface.

**[ES]** Los módulos de alto nivel no deben depender de los de bajo nivel. Ambos deben depender de abstracciones.

```
Domain:         IJobApplicationRepository (interface)
                         ↑ implemented by
Infrastructure: JobApplicationRepository (EF Core)
                         ↑ injected into
Application:    GetJobApplicationsHandler (uses IJobApplicationRepository)
```

---

## 3.4 Clean Architecture vs MVC vs N-Layer

| Aspect | Clean Architecture | MVC | N-Layer |
|--------|-------------------|-----|---------|
| Dependency rule | Strict inward | None | Sometimes |
| Testability | Excellent | Good | Moderate |
| Framework coupling | None in core | Tight | Moderate |
| Complexity | Higher | Low | Medium |
| Best for | Complex domains | Simple CRUD | Medium apps |

---

# SECTION 4: CQRS AND MEDIATR / CQRS y MediatR

---

## 4.1 What is CQRS

**[EN]** CQRS (Command Query Responsibility Segregation) separates read operations (Queries) from write operations (Commands). Each operation has its own model, handler, and validation logic. This improves clarity, scalability, and testability.

**[ES]** CQRS separa las operaciones de lectura (Queries) de las de escritura (Commands). Cada operación tiene su propio modelo, handler y validación.

| | Command | Query |
|--|---------|-------|
| Purpose | Change state | Read state |
| Returns | DTO or nothing | DTO or list |
| Example | `CreateJobApplicationCommand` | `GetJobApplicationsQuery` |
| Side effects | Yes (DB write) | No (read-only) |

---

## 4.2 MediatR and the Mediator Pattern

**[EN]** MediatR implements the Mediator pattern: instead of classes calling each other directly, they communicate through a central mediator. The controller sends a request to MediatR, which finds and invokes the correct handler.

**[ES]** MediatR implementa el patrón Mediator: en lugar de que las clases se llamen directamente, se comunican a través de un mediador central.

```
Controller → mediator.Send(command) → MediatR → Handler → Repository → DB
```

---

## 4.3 Code Examples — Ejemplos de Código

```csharp
// Command (Application/Commands/CreateJobApplicationCommand.cs)
public record CreateJobApplicationCommand(
    string Company,
    string Position,
    ApplicationStatus Status,
    string? Notes
) : IRequest<JobApplicationDto>;

// Handler (Application/Commands/CreateJobApplicationHandler.cs)
public class CreateJobApplicationHandler(
    IJobApplicationRepository repository,
    ICurrentUserService currentUser
) : IRequestHandler<CreateJobApplicationCommand, JobApplicationDto>
{
    public async Task<JobApplicationDto> Handle(
        CreateJobApplicationCommand request,
        CancellationToken cancellationToken)
    {
        var application = new JobApplication
        {
            Id = Guid.NewGuid(),
            UserId = currentUser.UserId,
            Company = request.Company,
            Position = request.Position,
            Status = request.Status,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow
        };

        await repository.AddAsync(application);
        return application.ToDto();
    }
}

// Validator (Application/Validators/CreateJobApplicationValidator.cs)
public class CreateJobApplicationValidator
    : AbstractValidator<CreateJobApplicationCommand>
{
    public CreateJobApplicationValidator()
    {
        RuleFor(x => x.Company).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Position).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Status).IsInEnum();
    }
}
```

---

## 4.4 Pipeline Behavior — FluentValidation Integration

**[EN]** A Pipeline Behavior is middleware for MediatR. It runs before/after every handler. FluentValidation is registered as a behavior so all commands are automatically validated before reaching the handler.

**[ES]** Un Pipeline Behavior es middleware para MediatR. Corre antes/después de cada handler.

```csharp
public class ValidationBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var context = new ValidationContext<TRequest>(request);
        var failures = validators
            .Select(v => v.Validate(context))
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Any())
            throw new ValidationException(failures);

        return await next(); // proceed to handler
    }
}
```

---

## 4.5 When NOT to use CQRS — Cuándo NO usar CQRS

**[EN]** CQRS adds complexity. Avoid it for: simple CRUD apps with no business logic, small teams maintaining simple APIs, apps where read/write models are identical. In JobTracker Pro it's appropriate because the project serves as a technical portfolio demonstrating the pattern.

**[ES]** CQRS agrega complejidad. Evítalo en: apps CRUD simples sin lógica de negocio, equipos pequeños con APIs simples.

---

# SECTION 5: DATABASE AND ORM / Base de Datos y ORM

---

## 5.1 SQL vs NoSQL

| | SQL (Relational) | NoSQL |
|--|-----------------|-------|
| Structure | Tables, rows, columns | Documents, key-value, graphs |
| Schema | Fixed | Flexible |
| ACID | ✅ Yes | Varies |
| Joins | ✅ Native | ❌ Manual |
| Scale | Vertical primarily | Horizontal primarily |
| Examples | PostgreSQL, SQL Server | MongoDB, Redis |
| Use when | Structured data, relationships | Flexible schema, high write volume |

**In JobTracker Pro:** PostgreSQL for persistent data (structured, relational). Redis for caching (key-value, TTL-based).

---

## 5.2 Entity Framework Core

**[EN]** EF Core is an ORM (Object-Relational Mapper) that maps C# classes to database tables. `DbContext` represents the database session. `DbSet<T>` represents a table. Migrations track schema changes as C# code.

**[ES]** EF Core es un ORM que mapea clases C# a tablas de base de datos. `DbContext` representa la sesión. `DbSet<T>` representa una tabla.

```csharp
public class AppDbContext(DbContextOptions<AppDbContext> options)
    : DbContext(options)
{
    public DbSet<JobApplication> JobApplications => Set<JobApplication>();
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<JobApplication>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Company).IsRequired().HasMaxLength(200);
            entity.HasOne(e => e.User)
                  .WithMany(u => u.JobApplications)
                  .HasForeignKey(e => e.UserId);
        });
    }
}
```

---

## 5.3 Repository Pattern

**[EN]** The Repository Pattern abstracts data access behind an interface. Controllers/Handlers never talk directly to EF Core — they use repository interfaces defined in the Domain layer.

**[ES]** El patrón Repository abstrae el acceso a datos detrás de una interfaz.

```
Handler → IJobApplicationRepository (Domain) → JobApplicationRepository (Infrastructure/EF Core) → PostgreSQL
```

**Benefits / Beneficios:** Testable (mock the interface), swappable (change DB without changing business logic).

---

## 5.4 N+1 Query Problem

**[EN]** N+1 occurs when you load N records and then execute 1 additional query per record to load related data. Fix: use `.Include()` to load related data in a single JOIN query.

**[ES]** N+1 ocurre cuando cargas N registros y ejecutas 1 consulta adicional por registro para datos relacionados.

```csharp
// ❌ N+1: 1 query for users + N queries for applications
var users = await dbContext.Users.ToListAsync();
foreach (var user in users)
    var apps = await dbContext.JobApplications.Where(a => a.UserId == user.Id).ToListAsync();

// ✅ Single JOIN query
var users = await dbContext.Users
    .Include(u => u.JobApplications)
    .ToListAsync();
```

---

## 5.5 Redis and Cache-Aside Pattern

**[EN]** Redis is an in-memory key-value store used for caching. The **cache-aside** pattern: check cache first → if miss, query DB → store result in cache → return result.

**[ES]** Redis es un almacén en memoria para caché. El patrón **cache-aside**: verificar caché → si no hay dato, consultar DB → guardar en caché → retornar.

```csharp
public async Task<IEnumerable<JobApplicationDto>> GetCachedApplicationsAsync(Guid userId)
{
    var cacheKey = $"applications:{userId}";
    var cached = await cacheService.GetAsync<IEnumerable<JobApplicationDto>>(cacheKey);

    if (cached is not null) return cached; // Cache HIT

    var result = await repository.GetAllByUserIdAsync(userId); // Cache MISS
    await cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5));
    return result;
}
```

---

## 5.6 EF Core Migrations — Migraciones

**[EN]** Migrations track schema changes as versioned C# code files. Applied automatically on startup via `db.Database.Migrate()`.

**[ES]** Las migraciones rastrean cambios de esquema como archivos C# versionados.

```bash
dotnet ef migrations add AddNotesColumn --project src/JobTrackerPro.Infrastructure
dotnet ef database update
```

**Guard in JobTracker Pro:**
```csharp
// Program.cs — only run migrations in non-testing environments
if (!app.Environment.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}
```

---

# SECTION 6: AUTHENTICATION AND SECURITY / Autenticación y Seguridad

---

## 6.1 Authentication vs Authorization

**[EN]** **Authentication** = Verifying WHO you are (login). **Authorization** = Verifying WHAT you can do (permissions).

**[ES]** **Autenticación** = Verificar QUIÉN eres (login). **Autorización** = Verificar QUÉ puedes hacer (permisos).

```
[401 Unauthorized] = Not authenticated (missing/invalid JWT)
[403 Forbidden]    = Authenticated but not authorized (valid JWT, wrong permissions)
```

---

## 6.2 JWT — JSON Web Token

**[EN]** A JWT is a self-contained, digitally signed token. It has 3 parts separated by dots, all Base64URL encoded: `Header.Payload.Signature`.

**[ES]** Un JWT es un token autocontenido y firmado digitalmente. Tiene 3 partes separadas por puntos.

```
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9
.eyJzdWIiOiJ1c2VyLWlkIiwiZW1haWwiOiJ1c2VyQGVtYWlsLmNvbSIsImV4cCI6MTcxMzAwMDAwMH0
.HMACSHA256(base64Header + "." + base64Payload, secret)
```

### JWT Claims / Claims del JWT

| Claim | Meaning [EN] | Significado [ES] |
|-------|-------------|-----------------|
| `sub` | Subject (user ID) | Sujeto (ID del usuario) |
| `iss` | Issuer | Emisor |
| `aud` | Audience | Audiencia |
| `exp` | Expiration timestamp | Expiración |
| `iat` | Issued at | Emitido en |

**In JobTracker Pro:** Access token expires in **60 minutes**. Refresh token expires in **7 days**, stored in PostgreSQL.

---

## 6.3 JWT Flow Diagram — Diagrama del Flujo JWT

```
LOGIN FLOW:
User → POST /auth/login {email, password}
     → Server: BCrypt.Verify(password, hashedPassword)
     → Server: Generate JWT (60min) + RefreshToken (7 days)
     → Server: Store RefreshToken in DB
     ← Response: { accessToken, refreshToken }
     → Client: Store in localStorage

AUTHENTICATED REQUEST:
Client → GET /api/jobapplications
         Authorization: Bearer {accessToken}
       → Server: Validate JWT signature + expiry
       → Server: Extract userId from claims
       ← Response: 200 OK + JSON data

REFRESH FLOW:
Client → POST /auth/refresh { refreshToken }
       → Server: Find RefreshToken in DB, check not expired
       → Server: Generate new JWT + new RefreshToken (rotate)
       → Server: Invalidate old RefreshToken
       ← Response: { newAccessToken, newRefreshToken }
```

---

## 6.4 BCrypt Password Hashing

**[EN]** BCrypt is a password hashing algorithm that applies a salt (random value) and a cost factor (work factor). Even if two users have the same password, their hashes are different. Increasing the cost factor makes brute-force attacks slower.

**[ES]** BCrypt es un algoritmo de hash de contraseñas que aplica sal (valor aleatorio) y un factor de costo. Aunque dos usuarios tengan la misma contraseña, sus hashes son diferentes.

```csharp
// Registration
var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password); // cost=12

// Login verification
bool isValid = BCrypt.Net.BCrypt.Verify(request.Password, storedHash);
```

**NEVER store plain text passwords / NUNCA almacenar contraseñas en texto plano.**

---

## 6.5 Rate Limiting

**[EN]** Rate limiting restricts the number of requests a client can make in a time window. In JobTracker Pro: 60 requests/minute general, 10 requests/minute on auth endpoints.

**[ES]** Rate limiting restringe el número de peticiones que un cliente puede hacer en una ventana de tiempo.

```csharp
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("auth", limiter =>
    {
        limiter.PermitLimit = 10;
        limiter.Window = TimeSpan.FromMinutes(1);
    });
});
```

---

## 6.6 OWASP Top 10 Relevant to JobTracker Pro

| Threat | Risk | Mitigation in Project |
|--------|------|----------------------|
| XSS | JWT stolen from localStorage | Content-Security-Policy headers |
| Broken Auth | Brute force login | Rate limiting on /auth |
| Injection | SQL injection | EF Core parameterized queries |
| CSRF | Forged requests | localStorage (not cookies) for JWT |
| Security Misconfiguration | Exposed secrets | GitHub Secrets + Azure env vars |
| Broken Access Control | User A sees User B's data | Always filter by `userId` from JWT claims |

---

# SECTION 7: FRONTEND FUNDAMENTALS / Fundamentos Frontend

---

## 7.1 React and the Virtual DOM

**[EN]** React is a UI library for building component-based interfaces. The Virtual DOM is an in-memory copy of the real DOM. When state changes, React calculates the minimum diff (reconciliation) and applies only the necessary updates to the real DOM.

**[ES]** React es una librería UI para construir interfaces basadas en componentes. El Virtual DOM es una copia en memoria del DOM real.

---

## 7.2 Components, Props, and State

```typescript
// Props: data passed from parent to child (immutable in child)
interface JobApplicationCardProps {
  application: JobApplication;
  onDelete: (id: string) => void;
}

// State: data managed within the component (mutable via setter)
const [applications, setApplications] = useState<JobApplication[]>([]);
const [isLoading, setIsLoading] = useState(false);
```

---

## 7.3 useEffect and Component Lifecycle

```typescript
// Runs ONCE on mount (empty dependency array)
useEffect(() => {
  fetchApplications();
}, []);

// Runs when filter changes
useEffect(() => {
  fetchApplications(filter);
}, [filter]);

// Cleanup on unmount
useEffect(() => {
  const subscription = subscribe();
  return () => subscription.unsubscribe(); // cleanup
}, []);
```

---

## 7.4 Context API in JobTracker Pro

**[EN]** Context API shares state across components without prop drilling. In JobTracker Pro: `AuthContext` (user ID, login/logout), `ToastContext` (notifications), `ThemeContext` (dark/light mode).

**[ES]** Context API comparte estado entre componentes sin prop drilling.

```typescript
// AuthContext usage
const { userId, login, logout } = useAuth();

// ThemeContext usage
const { theme, toggleTheme } = useTheme();
```

---

## 7.5 TypeScript in React

```typescript
// Interface vs Type
interface JobApplication {        // Prefer interface for object shapes
  id: string;
  company: string;
  status: number;
  appliedAt: string | null;
}

type Status = 'pending' | 'active' | 'closed';  // Prefer type for unions

// Generics
const [items, setItems] = useState<JobApplication[]>([]);
async function fetchData<T>(url: string): Promise<T> { ... }
```

---

## 7.6 ApplicationStatus in Frontend

**[EN]** The frontend `ApplicationStatus` const uses different numeric values than the backend enum. The DTO returns `status` as `int`, and the frontend's values are authoritative for display. **Do NOT sync them unless both sides are updated and tested.**

**[ES]** El `ApplicationStatus` del frontend usa valores numéricos diferentes al enum del backend.

```typescript
// frontend/src/types/index.ts
export const ApplicationStatus = {
  Saved: 0,
  Applied: 1,
  Screening: 2,
  TechnicalTest: 3,
  Interview: 4,
  OfferReceived: 5,
  Accepted: 6,
  Rejected: 7,
  Withdrawn: 8,
} as const;

export const STATUS_LABELS: Record<number, string> = {
  [ApplicationStatus.Saved]: 'Saved',
  [ApplicationStatus.Applied]: 'Applied',
  // ...
};
```

---

## 7.7 Tailwind CSS

**[EN]** Tailwind is a utility-first CSS framework. Instead of writing custom CSS, you apply predefined utility classes directly in JSX. Dark mode is enabled via the `dark:` prefix.

**[ES]** Tailwind es un framework CSS utility-first. En lugar de CSS personalizado, aplicas clases utilitarias predefinidas directamente en JSX.

```tsx
<div className="bg-white dark:bg-gray-800 rounded-lg shadow p-4 hover:shadow-md transition-shadow">
  <h2 className="text-lg font-semibold text-gray-900 dark:text-white">
    {application.company}
  </h2>
</div>
```

---

# SECTION 8: HTTP CLIENT AND STATE MANAGEMENT / Cliente HTTP y Estado

---

## 8.1 Axios vs Fetch

| Feature | Axios | fetch |
|---------|-------|-------|
| Interceptors | ✅ Built-in | ❌ Manual |
| Auto JSON parse | ✅ | ❌ Manual `.json()` |
| Error on 4xx/5xx | ✅ Throws | ❌ Only network errors |
| Request cancellation | ✅ AbortController | ✅ AbortController |
| Bundle size | ~13KB | Native |

---

## 8.2 Axios Interceptors — Token Refresh with Queue

**[EN]** When the access token expires, the interceptor catches the 401 response, queues pending requests, fetches a new token, then replays all queued requests with the new token.

**[ES]** Cuando expira el access token, el interceptor atrapa el 401, encola las peticiones pendientes, obtiene un nuevo token y reintenta todas.

```typescript
// frontend/src/api/axiosConfig.ts
let isRefreshing = false;
let failedQueue: Array<{
  resolve: (token: string) => void;
  reject: (error: unknown) => void;
}> = [];

const processQueue = (error: unknown, token: string | null = null) => {
  failedQueue.forEach(({ resolve, reject }) => {
    if (error) reject(error);
    else resolve(token!);
  });
  failedQueue = [];
};

axiosInstance.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;

    if (error.response?.status === 401 && !originalRequest._retry) {
      if (isRefreshing) {
        // Queue this request until refresh completes
        return new Promise((resolve, reject) => {
          failedQueue.push({ resolve, reject });
        }).then((token) => {
          originalRequest.headers.Authorization = `Bearer ${token}`;
          return axiosInstance(originalRequest);
        });
      }

      originalRequest._retry = true;
      isRefreshing = true;

      try {
        const refreshToken = localStorage.getItem('refreshToken');
        const { data } = await axios.post('/auth/refresh', { refreshToken });
        
        localStorage.setItem('accessToken', data.accessToken);
        processQueue(null, data.accessToken);
        
        originalRequest.headers.Authorization = `Bearer ${data.accessToken}`;
        return axiosInstance(originalRequest);
      } catch (err) {
        processQueue(err, null);
        logout(); // redirect to login
        return Promise.reject(err);
      } finally {
        isRefreshing = false;
      }
    }

    return Promise.reject(error);
  }
);
```

---

## 8.3 Client-side vs Server-side Filtering

| | Client-side | Server-side |
|--|-------------|-------------|
| When | Small datasets (<500 rows) | Large datasets |
| How | Filter in JS after fetch | SQL WHERE clause |
| Performance | Fast (no HTTP) | Scales better |
| In JobTracker Pro | Status filter in dashboard | Recommended for future pagination |

---

## 8.4 Pagination — Paginación

**[EN]** Offset/limit pagination: skip N records, take M. Simple but inefficient for large datasets. Cursor-based: use last record ID as cursor. More efficient for infinite scroll.

**[ES]** Paginación offset/limit: salta N registros, toma M. Simple pero ineficiente para grandes datasets.

```
GET /api/jobapplications?page=2&pageSize=10
GET /api/jobapplications?cursor=last-id&take=10
```

---

# SECTION 9: DEVOPS AND CI/CD / DevOps y CI/CD

---

## 9.1 CI/CD Pipeline in JobTracker Pro

**[EN]** CI/CD automates the build, test, and deploy cycle. Every push to `main` triggers the pipeline.

**[ES]** CI/CD automatiza el ciclo de construcción, pruebas y despliegue.

```yaml
# .github/workflows/ci.yml — Jobs in order:

1. build-and-test
   ├── dotnet restore
   ├── dotnet build
   ├── dotnet test (unit tests)
   └── dotnet test (integration tests)

2. docker-build
   └── docker build (validates Dockerfile, no push)

3. deploy-api
   ├── dotnet publish -c Release
   └── azure/webapps-deploy@v3 → Azure App Service

4. deploy-frontend
   ├── npm ci
   ├── npm run build (injects VITE_API_URL from GitHub Secret)
   └── peaceiris/actions-gh-pages@v4 → GitHub Pages
```

---

## 9.2 Docker

**[EN]** Docker packages the application and all its dependencies into a portable container. A **Dockerfile** defines how to build the image. `docker-compose.yml` orchestrates multiple containers (API + PostgreSQL + Redis).

**[ES]** Docker empaqueta la aplicación y sus dependencias en un contenedor portable.

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/JobTrackerPro.Api/JobTrackerPro.Api.csproj", "src/JobTrackerPro.Api/"]
RUN dotnet restore "src/JobTrackerPro.Api/JobTrackerPro.Api.csproj"
COPY . .
RUN dotnet publish "src/JobTrackerPro.Api/JobTrackerPro.Api.csproj" -c Release -o /app/publish

FROM base AS final
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "JobTrackerPro.Api.dll"]
```

---

## 9.3 Azure App Service vs GitHub Pages

| | Azure App Service | GitHub Pages |
|--|------------------|--------------|
| Type | PaaS (Platform as a Service) | Static hosting + CDN |
| Use in project | ASP.NET Core API (backend) | React SPA (frontend) |
| Scaling | Vertical + Horizontal | Auto CDN |
| Config | App Settings (env vars) | GitHub Secrets (build-time) |

---

## 9.4 Environment Variables and Secrets

**[EN]** Secrets (passwords, API keys) must NEVER be committed to Git. Store them in: GitHub Secrets (CI/CD), Azure App Service Configuration, local `.env` files (gitignored).

**[ES]** Los secrets NUNCA deben estar en Git.

```
GitHub Secrets → injected during CI/CD build
Azure App Service → Connection Strings + Application Settings
Local dev → appsettings.Development.json or .env (gitignored)
```

**Azure env var naming (nested keys):**
```
JwtSettings__Secret     (double underscore = nested JSON key)
JwtSettings__Issuer
```

---

## 9.5 Serverless — Neon.tech PostgreSQL

**[EN]** Serverless databases scale to zero when idle and scale up automatically on demand. Neon.tech provides serverless PostgreSQL. No server management, pay-per-use, but requires SSL connections.

**[ES]** Las bases de datos serverless escalan a cero cuando están inactivas. Neon.tech provee PostgreSQL serverless.

**Connection string format / Formato de cadena de conexión:**
```
Host=ep-xxx.us-east-2.aws.neon.tech;Database=neondb;Username=neondb_owner;Password=xxx;SSL Mode=Require;
```

---

# SECTION 10: TESTING / Pruebas

---

## 10.1 Testing Pyramid — Pirámide de Pruebas

```
        ┌───────────────┐
        │   E2E Tests   │  ← Few, slow, expensive (Playwright, Cypress)
        │               │
      ┌─┴───────────────┴─┐
      │ Integration Tests  │  ← Medium number (WebApplicationFactory)
      │                    │
    ┌─┴────────────────────┴─┐
    │      Unit Tests         │  ← Many, fast, cheap (xUnit + Moq)
    └─────────────────────────┘
```

---

## 10.2 Unit Tests with xUnit and Moq

**[EN]** Unit tests test a single class/method in isolation. Dependencies are replaced with mocks (fake implementations that return predefined values).

**[ES]** Los unit tests prueban una sola clase/método en aislamiento. Las dependencias se reemplazan con mocks.

```csharp
public class CreateJobApplicationHandlerTests
{
    private readonly Mock<IJobApplicationRepository> _repoMock = new();
    private readonly Mock<ICurrentUserService> _userMock = new();

    [Fact]
    public async Task Handle_ValidCommand_ReturnsDto()
    {
        // Arrange
        _userMock.Setup(u => u.UserId).Returns(Guid.NewGuid());
        _repoMock.Setup(r => r.AddAsync(It.IsAny<JobApplication>()))
                 .Returns(Task.CompletedTask);

        var handler = new CreateJobApplicationHandler(_repoMock.Object, _userMock.Object);
        var command = new CreateJobApplicationCommand("Microsoft", "Senior Dev", ApplicationStatus.Applied, null);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Company.Should().Be("Microsoft");
        _repoMock.Verify(r => r.AddAsync(It.IsAny<JobApplication>()), Times.Once);
    }
}
```

---

## 10.3 Integration Tests with WebApplicationFactory

**[EN]** Integration tests test the full HTTP pipeline. `WebApplicationFactory` starts the real ASP.NET Core app in memory, with InMemory EF Core instead of PostgreSQL.

**[ES]** Los integration tests prueban el pipeline HTTP completo con la app real en memoria.

```csharp
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            // Replace real DB with InMemory
            var descriptor = services.SingleOrDefault(d =>
                d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor != null) services.Remove(descriptor);

            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase("TestDb"));
        });
    }
}

public class JobApplicationsControllerTests(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>
{
    [Fact]
    public async Task GetApplications_WithValidToken_Returns200()
    {
        // Arrange
        var client = factory.CreateClient();
        var token = GenerateTestJwt();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.GetAsync("/api/jobapplications");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
```

---

## 10.4 Test Isolation — Aislamiento de Pruebas

**[EN]** Each test must start with a clean state. In JobTracker Pro integration tests, InMemory DB is recreated per test class (via `IClassFixture`). Unit tests use fresh mocks per test method.

**[ES]** Cada prueba debe iniciar con estado limpio.

**Why / Por qué:** If Test A creates data that Test B reads, tests are order-dependent and flaky.

---

# SECTION 11: INTERVIEW QUESTIONS / Preguntas de Entrevista

---

## Web Fundamentals (10 questions)

**[JUNIOR] Q1**
**EN:** What is the difference between GET and POST?
**ES:** ¿Cuál es la diferencia entre GET y POST?
**Answer:** GET retrieves data and is idempotent — calling it multiple times produces the same result with no side effects. POST creates/submits data and is not idempotent. GET parameters are in the URL; POST data is in the request body.
**Trap:** "Can GET have a body?" — Yes technically, but it's bad practice and many servers ignore it.

---

**[JUNIOR] Q2**
**EN:** What does stateless mean in REST?
**ES:** ¿Qué significa stateless en REST?
**Answer:** Each HTTP request contains all the information needed to process it. The server doesn't store client session state between requests. This is why JWTs include user identity — the server doesn't remember who you are between requests.
**Trap:** "Is REST always stateless?" — Yes, it's a core constraint. Sessions stored on the server violate REST principles.

---

**[JUNIOR] Q3**
**EN:** What is CORS and when does it trigger?
**ES:** ¿Qué es CORS y cuándo se activa?
**Answer:** CORS is a browser security mechanism that blocks cross-origin requests unless the server explicitly allows them. It triggers when JavaScript makes a request to a different origin (different domain, port, or protocol). The browser sends a preflight OPTIONS request first for non-simple requests.
**Trap:** "Does CORS protect the server?" — No, it protects users in the browser. A curl request ignores CORS completely.

---

**[MID] Q4**
**EN:** What is the difference between 401 and 403?
**ES:** ¿Cuál es la diferencia entre 401 y 403?
**Answer:** 401 Unauthorized means the request lacks valid authentication (no JWT, or invalid JWT). 403 Forbidden means the request is authenticated (valid JWT) but the user doesn't have permission to access that resource.
**Trap:** "Why is 401 called 'Unauthorized' if it means 'unauthenticated'?" — Historical naming confusion in the HTTP spec.

---

**[MID] Q5**
**EN:** What is the difference between localStorage, sessionStorage, and cookies?
**ES:** ¿Cuál es la diferencia entre localStorage, sessionStorage y cookies?
**Answer:** localStorage persists until manually cleared. sessionStorage clears when the tab closes. Cookies can have expiry dates and are automatically sent with every HTTP request to the same domain. Cookies with `httpOnly` flag are inaccessible to JavaScript (XSS-safe).
**Trap:** "Where should you store JWTs?" — No perfect answer: localStorage is XSS-vulnerable; httpOnly cookies are CSRF-vulnerable. Must pick your threat model.

---

**[MID] Q6**
**EN:** What is a SPA and what problem does it solve?
**ES:** ¿Qué es una SPA y qué problema resuelve?
**Answer:** A Single Page Application loads one HTML document and dynamically updates the UI via JavaScript. It solves the full-page reload problem in traditional web apps, providing a faster, more app-like experience. The tradeoff is initial load time (larger JS bundle) and SEO complexity.
**Trap:** "What's the problem with SPAs?" — Initial load is slow, SEO requires extra work (SSR or pre-rendering), and the browser back button must be handled explicitly.

---

**[MID] Q7**
**EN:** What is idempotency and which HTTP methods are idempotent?
**ES:** ¿Qué es la idempotencia y qué métodos HTTP son idempotentes?
**Answer:** An operation is idempotent if calling it multiple times produces the same result. GET, PUT, DELETE, HEAD are idempotent. POST is not (calling POST /orders twice creates two orders). PATCH may or may not be idempotent depending on implementation.
**Trap:** "Is DELETE idempotent?" — Yes: deleting a resource that doesn't exist should return 404, but the state of the system (resource gone) is the same.

---

**[SENIOR] Q8**
**EN:** Explain the full HTTPS handshake process.
**ES:** Explica el proceso completo del handshake HTTPS.
**Answer:** Client sends ClientHello with supported TLS versions and cipher suites. Server responds with its certificate (public key). Client verifies certificate against trusted CAs, generates a pre-master secret encrypted with server's public key. Both derive a symmetric session key from the pre-master secret. All subsequent communication uses symmetric encryption (AES).
**Trap:** "Why use symmetric encryption for the session if asymmetric is more secure?" — Asymmetric is computationally expensive; symmetric is 1000x faster for bulk data transfer.

---

**[SENIOR] Q9**
**EN:** What is the difference between authentication and authorization? Give a concrete example.
**ES:** ¿Cuál es la diferencia entre autenticación y autorización?
**Answer:** Authentication verifies identity (who you are). Authorization verifies permissions (what you can do). In JobTracker Pro: when a user sends a JWT, ASP.NET Core authenticates them by validating the token. When they try to access another user's job application, the system checks authorization — the handler verifies `application.UserId == currentUser.UserId` before returning data.
**Trap:** "Can you have authorization without authentication?" — In practice no. You can't control what someone can do if you don't know who they are.

---

**[SENIOR] Q10**
**EN:** What is a preflight request and when is it sent?
**ES:** ¿Qué es un preflight request y cuándo se envía?
**Answer:** A preflight is an OPTIONS request the browser automatically sends before a "non-simple" cross-origin request to check if the server allows it. Non-simple requests include: custom headers (like Authorization), methods other than GET/POST, or Content-Type other than text/plain. The server must respond with appropriate CORS headers for the actual request to proceed.
**Trap:** "Can you skip the preflight?" — Yes, only for "simple" requests (GET, HEAD, POST with standard Content-Type and no custom headers).

---

## C# / .NET (10 questions)

**[JUNIOR] Q11**
**EN:** What is the difference between `var` and explicit typing in C#?
**ES:** ¿Cuál es la diferencia entre `var` y tipado explícito en C#?
**Answer:** `var` uses type inference — the compiler determines the type at compile time based on the right-hand side. There is no performance difference; both are statically typed. `var` reduces verbosity but can reduce readability when the type isn't obvious from context.
**Trap:** "Is `var` the same as `dynamic`?" — No. `var` is resolved at compile time (static). `dynamic` is resolved at runtime (dynamic dispatch, much slower, no IntelliSense).

---

**[JUNIOR] Q12**
**EN:** What is the difference between `async void` and `async Task`?
**ES:** ¿Cuál es la diferencia entre `async void` y `async Task`?
**Answer:** `async Task` returns an awaitable task. Exceptions propagate properly. `async void` returns nothing and exceptions cannot be caught by callers — they crash the application. Use `async void` ONLY for event handlers (required by the delegate signature).
**Trap:** "When would you use async void?" — Only for event handlers. Never in web API code.

---

**[JUNIOR] Q13**
**EN:** What is Dependency Injection and why is it used?
**ES:** ¿Qué es la Inyección de Dependencias y por qué se usa?
**Answer:** DI is a pattern where a class receives its dependencies from an external container rather than creating them itself. Benefits: makes code testable (inject mocks), reduces coupling (depends on interface, not implementation), centralizes object creation and lifetime management.
**Trap:** "What's the difference between Scoped, Singleton, and Transient?" — Singleton: one instance for app lifetime. Scoped: one per HTTP request. Transient: new instance every time it's requested.

---

**[MID] Q14**
**EN:** What is the difference between a C# Class, Record, and Struct?
**ES:** ¿Cuál es la diferencia entre Class, Record y Struct en C#?
**Answer:** Class: reference type, mutable, reference equality. Record: reference type, immutable by default, value equality (two records with same values are equal), ideal for DTOs and Commands. Struct: value type, copied on assignment, no inheritance, ideal for small value types (Point, Money).
**Trap:** "Can Records be mutable?" — Yes, with `init` setters or `record class` with `set`. But immutability is the primary benefit.

---

**[MID] Q15**
**EN:** What is LINQ and how does it work with EF Core?
**ES:** ¿Qué es LINQ y cómo funciona con EF Core?
**Answer:** LINQ is a query language embedded in C# for querying collections. With EF Core, LINQ queries are translated to SQL at runtime by the query provider. The query is not executed until enumerated (`ToListAsync()`, `FirstOrDefaultAsync()`). This deferred execution allows query composition.
**Trap:** "When does EF Core execute the SQL query?" — On enumeration (ToList, FirstOrDefault, Count, etc.), not when building the query expression.

---

**[MID] Q16**
**EN:** What is middleware in ASP.NET Core and why does order matter?
**ES:** ¿Qué es el middleware en ASP.NET Core y por qué importa el orden?
**Answer:** Middleware is a component in the request/response pipeline. Each middleware can inspect, modify, short-circuit, or pass the request to the next component. Order matters because: CORS headers must be set before routing handles the request; authentication must run before authorization; exception handling must be first to catch errors from downstream middleware.
**Trap:** "What happens if you put UseAuthorization() before UseAuthentication()?" — Authorization checks will fail because the user identity hasn't been established yet.

---

**[MID] Q17**
**EN:** What is the Repository Pattern and why use it?
**ES:** ¿Qué es el patrón Repository y por qué usarlo?
**Answer:** The Repository Pattern abstracts data access behind an interface. Business logic talks to the interface, not directly to EF Core or any specific ORM. Benefits: testability (mock the interface in unit tests), swappability (change PostgreSQL to MongoDB without touching business logic), and single responsibility.
**Trap:** "Is Repository Pattern redundant with EF Core (which is already an abstraction)?" — This is a valid debate. In Clean Architecture, it maintains the Dependency Inversion Principle — Domain/Application don't reference EF Core at all.

---

**[SENIOR] Q18**
**EN:** Explain how the CLR manages memory in .NET.
**ES:** Explica cómo el CLR gestiona la memoria en .NET.
**Answer:** The CLR uses garbage collection (GC) with generational collection. Gen 0: newly allocated short-lived objects (collected frequently). Gen 1: objects that survived Gen 0. Gen 2: long-lived objects (collected infrequently). The GC runs automatically when memory pressure increases. For unmanaged resources, implement `IDisposable` and use `using` statements.
**Trap:** "Does .NET have memory leaks?" — Yes: event handlers holding references, static collections, improper IDisposable usage, uncancelled async operations.

---

**[SENIOR] Q19**
**EN:** What is the difference between `IEnumerable`, `IQueryable`, and `List`?
**ES:** ¿Cuál es la diferencia entre `IEnumerable`, `IQueryable` y `List`?
**Answer:** `List<T>`: in-memory collection, already loaded. `IEnumerable<T>`: lazy in-memory iteration, query executed in C#. `IQueryable<T>`: lazy, expression tree-based — EF Core can translate it to SQL. Use `IQueryable` when building database queries to avoid loading all data into memory before filtering.
**Trap:** "What happens if you return IEnumerable from a repository that uses EF Core?" — The query executes at the repository boundary. If you add `.Where()` after, it filters in C# (all rows loaded). With `IQueryable`, the filter is added to the SQL.

---

**[SENIOR] Q20**
**EN:** What is the N+1 query problem and how do you solve it in EF Core?
**ES:** ¿Qué es el problema N+1 y cómo se resuelve en EF Core?
**Answer:** N+1 occurs when loading N parent records generates N additional queries to load their children. Example: loading 100 users then querying applications per user = 101 queries. Solution: `.Include()` generates a single JOIN query. Alternative: `.AsSplitQuery()` for complex scenarios to avoid cartesian explosion.
**Trap:** "Can `.Include()` cause its own problems?" — Yes: cartesian explosion when including multiple collections (100 users × 50 applications × 10 tags = 50,000 rows). Use `.AsSplitQuery()` in those cases.

---

## Architecture & Design Patterns (10 questions)

**[JUNIOR] Q21**
**EN:** What is Clean Architecture?
**ES:** ¿Qué es la Arquitectura Limpia?
**Answer:** Clean Architecture organizes code in concentric layers where dependencies point inward only. The Domain layer (core business) has no external dependencies. The Application layer depends only on Domain. Infrastructure implements interfaces defined in inner layers. This makes business logic independent of frameworks, databases, and UI.
**Trap:** "What's the main benefit of Clean Architecture?" — Testability and independence from external concerns. You can swap PostgreSQL for MongoDB without changing a single line of business logic.

---

**[JUNIOR] Q22**
**EN:** What is the Single Responsibility Principle?
**ES:** ¿Qué es el Principio de Responsabilidad Única?
**Answer:** Every class should have only one reason to change. A class that handles HTTP requests, validates data, queries the database, AND sends emails violates SRP — it has too many responsibilities. In JobTracker Pro, this is why there are separate: validators, handlers, repositories, and services.
**Trap:** "What's a 'God class'?" — A class that does everything, violating SRP. One of the errors specifically called out in the LinkedInAgent.Grpc analysis.

---

**[MID] Q23**
**EN:** What is CQRS and when should you use it?
**ES:** ¿Qué es CQRS y cuándo debes usarlo?
**Answer:** CQRS separates read and write models. Commands change state; Queries read state. Use it when: read and write models are significantly different, you need separate scaling for reads and writes, or the domain is complex enough to benefit from explicit command/query separation. Avoid for simple CRUD apps where it adds unnecessary complexity.
**Trap:** "Does CQRS require Event Sourcing?" — No. They're often used together but are independent patterns.

---

**[MID] Q24**
**EN:** What is the Mediator pattern and what problem does it solve?
**ES:** ¿Qué es el patrón Mediator y qué problema resuelve?
**Answer:** Mediator centralizes communication between objects so they don't reference each other directly. Without it, controllers would directly reference handler classes, creating tight coupling. With MediatR, the controller only knows about the request/response types — the mediator finds and invokes the correct handler.
**Trap:** "What's a downside of MediatR?" — Indirection makes it harder to trace code flow. You can't "click to definition" from a Send() call to find the handler without searching.

---

**[MID] Q25**
**EN:** What is the Dependency Inversion Principle?
**ES:** ¿Qué es el Principio de Inversión de Dependencias?
**Answer:** High-level modules should not depend on low-level modules. Both should depend on abstractions. In practice: the Application layer defines `IJobApplicationRepository`. Infrastructure implements it. The Application layer doesn't know about EF Core or PostgreSQL — it only knows the interface.
**Trap:** "What's the difference between Dependency Inversion and Dependency Injection?" — DIP is a design principle (about abstractions). DI is a technique for implementing DIP (how you get the implementation into the class).

---

**[MID] Q26**
**EN:** What is a Pipeline Behavior in MediatR?
**ES:** ¿Qué es un Pipeline Behavior en MediatR?
**Answer:** A Pipeline Behavior is middleware that wraps every MediatR request. It runs before and/or after the handler. In JobTracker Pro, `ValidationBehavior` runs FluentValidation on every Command before it reaches the handler — removing the need to call validators manually in each handler.
**Trap:** "What are other uses for Pipeline Behaviors?" — Logging (log every command), performance monitoring (measure handler execution time), retry policies, transaction management.

---

**[SENIOR] Q27**
**EN:** Compare Clean Architecture with MVC and N-Layer. When would you choose each?
**ES:** Compara Clean Architecture con MVC y N-Layer.
**Answer:** MVC: simple, fast to develop, good for small apps with little business logic. N-Layer: separates Presentation/Business/Data layers but dependencies can go both ways. Clean Architecture: strict dependency rule, excellent for complex domains, highest upfront cost. Choose MVC for prototypes, N-Layer for medium apps, Clean Architecture for complex domains or portfolio projects demonstrating patterns.
**Trap:** "Is Clean Architecture always better?" — No. It's over-engineering for simple CRUD apps. The boilerplate cost (Commands, Handlers, Validators per use case) is only justified at sufficient complexity.

---

**[SENIOR] Q28**
**EN:** What is the Unit of Work pattern and how does it relate to EF Core's DbContext?
**ES:** ¿Qué es el patrón Unit of Work y cómo se relaciona con DbContext de EF Core?
**Answer:** Unit of Work tracks all changes made to objects during a business transaction and coordinates writing them out as a single atomic unit. EF Core's `DbContext` is already an implementation of Unit of Work — it tracks all entity changes and `SaveChanges()` commits them in a single transaction.
**Trap:** "Do you need a custom Unit of Work if you're using EF Core?" — Generally no. EF Core's DbContext handles it. Custom UoW is useful when coordinating changes across multiple DbContexts or external systems.

---

**[SENIOR] Q29**
**EN:** How does the Dependency Inversion Principle enable testing?
**ES:** ¿Cómo habilita el DIP las pruebas unitarias?
**Answer:** Because high-level modules depend on interfaces (abstractions), you can substitute real implementations with mocks in tests. In JobTracker Pro: `CreateJobApplicationHandler` depends on `IJobApplicationRepository`. In unit tests, we provide a `Mock<IJobApplicationRepository>` that returns predefined data without touching the database. Without interfaces, you'd need a real database for every unit test.
**Trap:** "Can you test without interfaces?" — You can test with a real DB (integration tests) but unit tests become impossible to isolate without interfaces/abstractions.

---

**[SENIOR] Q30**
**EN:** What are the tradeoffs of storing JWT in localStorage vs httpOnly cookies?
**ES:** ¿Cuáles son los tradeoffs de guardar JWT en localStorage vs cookies httpOnly?
**Answer:** localStorage: accessible to JavaScript (convenient for SPA), but vulnerable to XSS attacks — malicious JS can steal the token. httpOnly cookies: inaccessible to JavaScript (XSS-safe), but vulnerable to CSRF — malicious sites can trigger requests that include the cookie. The choice depends on threat model: if XSS is the bigger risk, use httpOnly cookies + CSRF tokens. JobTracker Pro uses localStorage, accepting XSS risk in exchange for simpler implementation and no CSRF concern.
**Trap:** "Is there a 100% safe option?" — No. Both have tradeoffs. httpOnly cookies + SameSite=Strict + CSRF tokens is currently the most widely recommended approach.

---

## Database & EF Core (10 questions)

**[JUNIOR] Q31**
**EN:** What is the difference between SQL and NoSQL databases?
**ES:** ¿Cuál es la diferencia entre bases de datos SQL y NoSQL?
**Answer:** SQL databases use structured tables with fixed schemas, support ACID transactions, and excel at relational data with complex joins. NoSQL databases offer flexible schemas, horizontal scaling, and come in multiple types (document, key-value, graph). Use SQL for structured data with relationships; NoSQL for flexible schemas, high write volume, or specific data access patterns.
**Trap:** "Is NoSQL faster than SQL?" — Not universally. Redis (NoSQL) is faster for key-value lookups. PostgreSQL may be faster for complex queries with indexes.

---

**[JUNIOR] Q32**
**EN:** What is a primary key and a foreign key?
**ES:** ¿Qué es una clave primaria y una clave foránea?
**Answer:** A primary key uniquely identifies each row in a table (usually a UUID or integer). A foreign key is a column that references the primary key of another table, establishing a relationship. In JobTracker Pro: `JobApplications.UserId` is a foreign key referencing `Users.Id`.
**Trap:** "Can a foreign key be null?" — Yes, if the relationship is optional. A null foreign key means "no related record."

---

**[MID] Q33**
**EN:** What is the N+1 query problem in EF Core?
**ES:** ¿Qué es el problema N+1 en EF Core?
**Answer:** N+1 occurs when loading a collection of entities and then accessing a navigation property of each entity, triggering one SQL query per entity instead of a single JOIN. Solution: use `.Include()` to eager-load related entities in one query, or `.AsSplitQuery()` for multiple collection includes.
**Trap:** "Does `.Include()` always solve N+1?" — Not if you do manual loops after loading. Also, with multiple `.Include()` on collections, it can cause cartesian explosion (N*M rows).

---

**[MID] Q34**
**EN:** What is the difference between `SaveChanges()` and a database transaction?
**ES:** ¿Cuál es la diferencia entre `SaveChanges()` y una transacción de base de datos?
**Answer:** `SaveChanges()` wraps all pending changes in a single implicit transaction — either all succeed or all fail. Explicit transactions (`dbContext.Database.BeginTransactionAsync()`) are needed when coordinating multiple `SaveChanges()` calls or mixing EF Core operations with raw SQL within one atomic unit.
**Trap:** "Is SaveChanges() always atomic?" — Within a single call, yes. But if you call SaveChanges() twice, the second call can fail after the first succeeded — you need explicit transactions to span them atomically.

---

**[MID] Q35**
**EN:** What is Redis and what is it used for in JobTracker Pro?
**ES:** ¿Qué es Redis y para qué se usa en JobTracker Pro?
**Answer:** Redis is an in-memory key-value store used primarily for caching. In JobTracker Pro it caches job application lists per user with a TTL (time-to-live). The cache-aside pattern is used: check cache first, if miss query DB and populate cache. Falls back gracefully if Redis connection is unavailable.
**Trap:** "What happens if Redis is down?" — The app should handle it gracefully, falling through to the database. Redis should be a performance optimization, not a required dependency.

---

**[MID] Q36**
**EN:** Why is `Status` returned as `int` in `JobApplicationDto` instead of a string?
**ES:** ¿Por qué `Status` se retorna como `int` en el DTO en lugar de string?
**Answer:** The frontend `ApplicationStatus` const object uses numeric values for filtering comparisons (`a.status === filter`). If the API returned `"Applied"` as a string, these comparisons would fail. Returning the integer keeps the contract explicit and avoids string parsing on the frontend.
**Trap:** "Wouldn't an enum string be more readable?" — More readable but breaks the existing frontend filter logic. Never change serialization format without updating both sides.

---

**[SENIOR] Q37**
**EN:** How do EF Core migrations work under the hood?
**ES:** ¿Cómo funcionan las migraciones de EF Core internamente?
**Answer:** When you run `dotnet ef migrations add`, EF Core compares the current model snapshot with the previous snapshot and generates C# code describing the difference. The `__EFMigrationsHistory` table tracks which migrations have been applied. `Database.Migrate()` applies all pending migrations in order. Each migration has an `Up()` (apply) and `Down()` (rollback) method.
**Trap:** "What happens if you delete a migration file that's already applied to production?" — EF Core will think the migration was never run and try to apply it again, causing errors. Never delete migrations that have been applied to a production database.

---

**[SENIOR] Q38**
**EN:** Explain the cache-aside pattern and its tradeoffs.
**ES:** Explica el patrón cache-aside y sus tradeoffs.
**Answer:** Cache-aside (lazy loading): application checks cache → miss → query DB → populate cache → return. Benefits: only caches what's needed, cache failure is non-fatal, data freshness control via TTL. Tradeoffs: cache miss penalty (double round-trip), stale data during TTL window, cache stampede on expiry (multiple requests all miss simultaneously and query DB).
**Trap:** "How do you handle cache invalidation?" — The hardest problem in computer science. Options: TTL-based expiry, explicit invalidation on write operations, write-through caching (update cache on every write).

---

**[SENIOR] Q39**
**EN:** What is connection pooling and why is it important?
**ES:** ¿Qué es el connection pooling y por qué es importante?
**Answer:** Connection pooling reuses existing database connections instead of creating a new one for each request. Creating a TCP connection + TLS handshake + authentication is expensive (~100ms). Npgsql (PostgreSQL .NET driver) maintains a pool of open connections that are leased and returned. In serverless environments (Neon.tech), connection pooling (via PgBouncer) is critical to avoid hitting connection limits.
**Trap:** "What happens if connection pool is exhausted?" — Requests queue waiting for a connection. If no connection becomes available in time, requests fail with a timeout error.

---

**[SENIOR] Q40**
**EN:** Why does JobTracker Pro use `IsEnvironment("Testing")` instead of `IsRelational()` as a migration guard?
**ES:** ¿Por qué JobTracker Pro usa `IsEnvironment("Testing")` en lugar de `IsRelational()` como guard de migraciones?
**Answer:** `IsRelational()` returns `true` even for InMemory EF Core when using `UseInternalServiceProvider`, causing migrations to attempt to run against the InMemory database (which doesn't support them). `IsEnvironment("Testing")` correctly identifies the test environment regardless of the database provider. This was a discovered bug fixed during development.
**Trap:** "Can you run migrations against InMemory database?" — No. InMemory database has no schema, no SQL, and no migration support. It's a pure in-memory object graph.

---

## Auth & Security (10 questions)

**[JUNIOR] Q41**
**EN:** What is JWT and what are its three parts?
**ES:** ¿Qué es JWT y cuáles son sus tres partes?
**Answer:** JWT (JSON Web Token) is a self-contained token for stateless authentication. Three parts separated by dots: Header (algorithm + type, Base64URL encoded), Payload (claims like userId, email, expiry, Base64URL encoded), Signature (HMAC-SHA256 of header.payload using secret key). The signature ensures the token hasn't been tampered with.
**Trap:** "Is JWT encrypted?" — No by default. It's encoded (Base64URL), not encrypted. Anyone can decode the payload. Never put sensitive data in JWT claims. Use JWE (JSON Web Encryption) if encryption is needed.

---

**[JUNIOR] Q42**
**EN:** Why do we need a refresh token if we have a JWT?
**ES:** ¿Por qué necesitamos un refresh token si tenemos JWT?
**Answer:** Short-lived access tokens (60 min) minimize exposure if stolen. But asking users to log in every hour is bad UX. Refresh tokens (7 days) are long-lived, stored in the database, and allow generating new access tokens without credentials. When compromised, refresh tokens can be individually revoked from the database.
**Trap:** "Can a refresh token be a JWT?" — Yes, but then you lose the ability to revoke it (JWTs are stateless). Database-stored refresh tokens can be revoked individually.

---

**[MID] Q43**
**EN:** Why do we use BCrypt instead of MD5 or SHA-256 for password hashing?
**ES:** ¿Por qué usamos BCrypt en lugar de MD5 o SHA-256 para hash de contraseñas?
**Answer:** MD5 and SHA-256 are fast hashing algorithms — good for data integrity but bad for passwords. An attacker with a GPU can try billions of combinations per second. BCrypt is deliberately slow (cost factor), has a built-in random salt (prevents rainbow table attacks), and the cost factor can be increased as hardware improves.
**Trap:** "Can you just add your own salt to SHA-256?" — Better than nothing but still much faster than BCrypt. Use Argon2 or BCrypt which are purpose-built for password hashing.

---

**[MID] Q44**
**EN:** What is XSS and how does it relate to JWT storage?
**ES:** ¿Qué es XSS y cómo se relaciona con el almacenamiento de JWT?
**Answer:** XSS (Cross-Site Scripting) is when malicious JavaScript is injected into a page and executed in the victim's browser. If JWT is stored in localStorage, XSS can read and exfiltrate it. Mitigation: Content-Security-Policy headers, input sanitization, httpOnly cookies (JS can't access them).
**Trap:** "Does using httpOnly cookies completely prevent XSS?" — It prevents token theft via XSS, but XSS can still do damage (make API calls in the user's session, modify the DOM, steal form data).

---

**[MID] Q45**
**EN:** What is CSRF and why doesn't it apply when using localStorage for JWT?
**ES:** ¿Qué es CSRF y por qué no aplica cuando usas localStorage para JWT?
**Answer:** CSRF (Cross-Site Request Forgery) tricks a browser into making authenticated requests to a site using its stored cookies. Since cookies are automatically sent with requests, a malicious page can forge requests to your API. localStorage is NOT automatically sent — the JavaScript must explicitly read and attach the token. So CSRF doesn't apply to localStorage-based JWT auth (but XSS does).
**Trap:** "Is localStorage always safer than cookies?" — No. It depends on the threat. localStorage: safe from CSRF, vulnerable to XSS. Cookies: safe from XSS (if httpOnly), vulnerable to CSRF (unless SameSite=Strict + CSRF tokens).

---

**[SENIOR] Q46**
**EN:** How would you implement token rotation for refresh tokens?
**ES:** ¿Cómo implementarías la rotación de refresh tokens?
**Answer:** On each refresh, issue a new refresh token and invalidate the old one in the database. This limits the window of exploitation if a refresh token is stolen. Implement refresh token families: if a token is reused (old token used again after rotation), revoke the entire family (all tokens for that user), forcing re-login.
**Trap:** "What happens in a refresh token reuse attack?" — The attacker steals a refresh token before it's rotated. If the legitimate user refreshes first, the attacker's token is now invalid. But if the attacker refreshes first, the legitimate user gets a 401 and must re-login.

---

**[SENIOR] Q47**
**EN:** What rate limiting strategy would you use to protect a login endpoint?
**ES:** ¿Qué estrategia de rate limiting usarías para proteger un endpoint de login?
**Answer:** Fixed window per IP (10 req/min) as first layer. Sliding window for smoother enforcement. Progressive delay (exponential backoff) after failures. Account lockout after N failed attempts. CAPTCHA after threshold. In JobTracker Pro: 10 requests/minute on auth endpoints via ASP.NET Core's built-in rate limiter.
**Trap:** "What if the attacker uses distributed IPs?" — Rate limit by IP + rate limit by username. Distributed attacks need WAF (Web Application Firewall) level protection.

---

**[SENIOR] Q48**
**EN:** How do you prevent a user from accessing another user's job applications?
**ES:** ¿Cómo evitas que un usuario acceda a las aplicaciones de otro usuario?
**Answer:** Never trust URL parameters for authorization. Always extract the userId from the JWT claims (server-side, unforgeable), then filter database queries by that userId. In JobTracker Pro: `currentUser.UserId` comes from JWT claims, and every query includes `.Where(a => a.UserId == currentUser.UserId)`. A user requesting `/api/jobapplications/{otherId}` gets 404 or 403.
**Trap:** "What's the vulnerability called when you can access other users' resources by changing an ID?" — IDOR (Insecure Direct Object Reference). One of the most common API vulnerabilities.

---

**[SENIOR] Q49**
**EN:** What is the `sub` claim in JWT and why is it important?
**ES:** ¿Qué es el claim `sub` en JWT y por qué es importante?
**Answer:** `sub` (subject) is the unique identifier of the JWT's subject — typically the user's ID. It's the primary claim used to identify who is making the request. In JobTracker Pro, `currentUser.UserId` is extracted from the `sub` claim. It's important because it binds the token to a specific user.
**Trap:** "Should you use email as the sub claim?" — Avoid it. Emails can change. Use an immutable identifier (UUID/GUID) as the sub claim.

---

**[SENIOR] Q50**
**EN:** Explain the full login → access token expiry → refresh flow in JobTracker Pro.
**ES:** Explica el flujo completo login → expiración del access token → refresh en JobTracker Pro.
**Answer:** 1) User POSTs credentials → server validates BCrypt hash → generates JWT (60min) + RefreshToken (7 days, stored in DB) → returns both to client → client stores in localStorage. 2) Each API request sends JWT in Authorization header → ASP.NET Core validates signature + expiry → extracts userId from claims. 3) On 401 response, Axios interceptor catches it → queues pending requests → POSTs refresh token to `/auth/refresh` → server validates DB record → issues new JWT + rotated RefreshToken → Axios replays queued requests with new token.
**Trap:** "What if the refresh token is also expired?" — Server returns 401, Axios interceptor calls `logout()`, clears localStorage, redirects to login page.

---

## React & Frontend (10 questions)

**[JUNIOR] Q51**
**EN:** What is the Virtual DOM and why does React use it?
**ES:** ¿Qué es el Virtual DOM y por qué React lo usa?
**Answer:** The Virtual DOM is a lightweight in-memory copy of the real DOM. When state changes, React creates a new Virtual DOM tree, diffs it against the previous one (reconciliation), and applies only the minimal necessary changes to the real DOM. Direct DOM manipulation is expensive; the diffing algorithm minimizes DOM operations.
**Trap:** "Is the Virtual DOM always faster?" — Not necessarily. For simple apps, direct DOM manipulation can be faster. The Virtual DOM adds overhead but provides better developer ergonomics and predictable performance at scale.

---

**[JUNIOR] Q52**
**EN:** What is the difference between props and state in React?
**ES:** ¿Cuál es la diferencia entre props y state en React?
**Answer:** Props are immutable data passed from parent to child component — the child can read but not modify them. State is mutable data owned by the component itself — the component controls it via `useState`. When either props or state changes, React re-renders the component.
**Trap:** "Can a child component modify its parent's state?" — Not directly. The parent passes a callback function as a prop, and the child calls it to request a state change.

---

**[JUNIOR] Q53**
**EN:** What is `useEffect` used for?
**ES:** ¿Para qué se usa `useEffect`?
**Answer:** `useEffect` handles side effects in functional components: fetching data, subscribing to events, manipulating the DOM, setting up timers. It runs after every render by default. Dependencies array controls when it re-runs: empty `[]` means once on mount; `[dep1, dep2]` means when those values change. Return a cleanup function to run on unmount.
**Trap:** "What happens if you don't include a dependency in the array?" — Stale closure: the effect uses an outdated value of that variable. React's ESLint rules (react-hooks/exhaustive-deps) catch this.

---

**[MID] Q54**
**EN:** When would you use Context API vs a state management library like Redux?
**ES:** ¿Cuándo usarías Context API vs Redux?
**Answer:** Context API: low-frequency updates (theme, auth, user preferences). Good for small-medium apps. Redux/Zustand: high-frequency updates, complex state logic, time-travel debugging, large teams needing structure. In JobTracker Pro, Context API is sufficient: `AuthContext` updates only on login/logout, `ThemeContext` only on theme toggle.
**Trap:** "Does Context cause performance problems?" — Yes if misused. Every component consuming a context re-renders when that context value changes. Solution: split contexts (AuthContext separate from frequently-updating state).

---

**[MID] Q55**
**EN:** What is TypeScript and what problem does it solve in React development?
**ES:** ¿Qué es TypeScript y qué problema resuelve en el desarrollo React?
**Answer:** TypeScript adds static typing to JavaScript. In React development, it catches type errors at compile time (before runtime), enables better IDE autocompletion and refactoring, makes component APIs explicit via interface props, and documents intent. It eliminates a whole class of "undefined is not a function" runtime errors.
**Trap:** "Does TypeScript make your app safer at runtime?" — No. TypeScript types are erased at runtime (after compilation). TypeScript only provides compile-time safety. Runtime validation still requires tools like Zod or manual checks.

---

**[MID] Q56**
**EN:** How does Axios handle automatic token refresh in JobTracker Pro?
**ES:** ¿Cómo maneja Axios la renovación automática del token en JobTracker Pro?
**Answer:** A response interceptor catches 401 responses. If not already refreshing, it sets `isRefreshing = true`, queues all other concurrent requests, and calls `/auth/refresh`. On success, processes the queue with the new token, replays the original request. On failure, calls logout(). The queue prevents multiple simultaneous refresh calls.
**Trap:** "What happens if two requests expire simultaneously?" — Without the queue, both would try to refresh → double-refresh → second refresh fails (token already rotated). The queue pattern handles this: only one refresh happens, others wait.

---

**[MID] Q57**
**EN:** What is a ProtectedRoute in React Router?
**ES:** ¿Qué es un ProtectedRoute en React Router?
**Answer:** A ProtectedRoute is a wrapper component that checks if the user is authenticated before rendering the protected page. If not authenticated, it redirects to the login page. In JobTracker Pro, it reads `userId` from `AuthContext` — if null, redirects to `/login`.
**Trap:** "Is client-side route protection secure?" — No, it's only UX protection. A user can bypass it by manipulating JavaScript. Real security must be enforced server-side on every API call.

---

**[SENIOR] Q58**
**EN:** What is the `as const` pattern in TypeScript and why is it used for ApplicationStatus?
**ES:** ¿Qué es el patrón `as const` en TypeScript y por qué se usa para ApplicationStatus?
**Answer:** `as const` makes an object's values literal types instead of broadened types. Without it, `ApplicationStatus.Saved` would be type `number`. With `as const`, it's type `0`. This enables TypeScript to use the values in discriminated unions, ensures the values can't be accidentally changed, and allows `typeof ApplicationStatus[keyof typeof ApplicationStatus]` as a type.
**Trap:** "Why not use a TypeScript `enum` for this?" — TypeScript enums have quirks: numeric enums allow reverse mapping, const enums are erased at runtime. `as const` objects are more predictable and work better with serialization.

---

**[SENIOR] Q59**
**EN:** What is the stale data problem in SPAs and how does JobTracker Pro address it?
**ES:** ¿Qué es el problema de datos obsoletos en SPAs y cómo lo aborda JobTracker Pro?
**Answer:** In SPAs, data is fetched once and cached in component state. If the backend data changes (another tab, another user, background job), the UI shows stale data. JobTracker Pro addresses this with: a stale data banner (shows warning if data hasn't been refreshed recently), the `StaleNotificationService` background job sends email alerts, and Redis cache has a TTL to force eventual re-fetch.
**Trap:** "What's the modern solution to stale data in React?" — React Query / TanStack Query provides automatic background refetching, stale-while-revalidate strategies, and cache invalidation out of the box.

---

**[SENIOR] Q60**
**EN:** How would you optimize the frontend for a user with 1,000 job applications?
**ES:** ¿Cómo optimizarías el frontend para un usuario con 1,000 aplicaciones?
**Answer:** Server-side pagination (don't load all 1,000 at once). Virtual scrolling (only render visible rows — react-virtual). Debounced search (don't query on every keystroke). Move filtering to server-side (SQL WHERE vs JS Array.filter). Index database columns used for filtering. Memoize expensive computations with `useMemo`. Consider React Query for intelligent caching and background updates.
**Trap:** "Is client-side filtering always bad?" — No: fast for small datasets (<200 rows), no extra API calls, instant UX. The threshold depends on data size and user expectations.

---

# SECTION 12: MENTAL MAPS / Mapas Mentales ASCII

---

## Map 1: Clean Architecture Layers

```
┌─────────────────────────────────────────────────────────────────┐
│                        API LAYER                                │
│  Controllers · Middleware · Program.cs · DI Registration        │
│  Rate Limiter · Exception Handler · Serilog · AppInsights       │
│                                                                 │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │               INFRASTRUCTURE LAYER                      │   │
│  │  AppDbContext · Migrations · Repositories (EF Core)     │   │
│  │  JwtService · RedisCacheService · SmtpEmailService      │   │
│  │  StaleNotificationService (BackgroundService)           │   │
│  │                                                         │   │
│  │  ┌───────────────────────────────────────────────┐     │   │
│  │  │            APPLICATION LAYER                  │     │   │
│  │  │  Commands · Queries · Handlers (MediatR)      │     │   │
│  │  │  DTOs · FluentValidation Validators           │     │   │
│  │  │  Pipeline Behaviors · ICurrentUserService     │     │   │
│  │  │                                               │     │   │
│  │  │  ┌─────────────────────────────────────┐     │     │   │
│  │  │  │         DOMAIN LAYER                │     │     │   │
│  │  │  │  Entities: JobApplication, User     │     │     │   │
│  │  │  │  Enums: ApplicationStatus           │     │     │   │
│  │  │  │  Interfaces: IJobApplicationRepo    │     │     │   │
│  │  │  │  Value Objects                      │     │     │   │
│  │  │  │  NO external dependencies           │     │     │   │
│  │  │  └─────────────────────────────────────┘     │     │   │
│  │  └───────────────────────────────────────────────┘     │   │
│  └─────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────┘

DEPENDENCY RULE: arrows point INWARD ONLY
API → Infrastructure → Application → Domain
```

---

## Map 2: JWT Lifecycle

```
CREATE (Login)                    STORE                    SEND
──────────────                   ───────                  ──────
User submits             Client stores in          Every API request:
email + password    →    localStorage:         →   Authorization: Bearer {token}
        │                  accessToken              
        ▼                  refreshToken              
BCrypt.Verify()                                    VALIDATE
        │                                          ──────────
        ▼                                          Server:
Generate JWT:                                      1. Decode header+payload
  Header: {alg:HS256}                              2. Verify signature (HMAC)
  Payload: {sub, email, exp}  ◄──────────────────  3. Check exp > now
  Signature: HMAC(h.p, secret)                     4. Extract userId
        │                                          5. Process request
        ▼
Store RefreshToken in DB
(hashed, userId, expiry)

        
REFRESH (on 401)                                   EXPIRE
──────────────────                                 ───────
Axios interceptor           →   Access Token:      60 minutes
catches 401 response            expired? → 401     
        │                                          Refresh Token:
        ▼                                          expired? → logout
POST /auth/refresh                                 (7 days)
{refreshToken: "..."}       
        │                   
        ▼                   
Validate in DB              
Not expired?                
Rotate: invalidate old      
Generate: new JWT           
         new RefreshToken   
        │                   
        ▼                   
Return {accessToken,        
        refreshToken}       
        │                   
        ▼                   
Replay queued requests      
```

---

## Map 3: CQRS Flow (MediatR)

```
HTTP Request
     │
     ▼
[Controller]
  ├── mediator.Send(CreateJobApplicationCommand {...})
  │              OR
  └── mediator.Send(GetJobApplicationsQuery {...})
                  │
                  ▼
          [MediatR Pipeline]
                  │
                  ▼ (runs first)
    [ValidationBehavior<TRequest>]
          │            │
     Validators    No validators
      exist?         found
          │            │
     ┌────┘            │
     ▼                 ▼
Validate           Proceed
     │
     ├── INVALID → throw ValidationException → 422 Response
     │
     └── VALID → next()
                    │
                    ▼
          [Logging Behavior] (optional)
                    │
                    ▼
          [Command/Query Handler]
          IRequestHandler<TRequest, TResponse>
                    │
                    ▼
          [Repository Interface]
          IJobApplicationRepository
                    │
                    ▼
          [EF Core Repository]
          (Infrastructure Layer)
                    │
                    ▼
          [PostgreSQL / Redis]
                    │
                    ▼
          Return Entity / List
                    │
                    ▼
          Map to DTO
                    │
                    ▼
          Handler returns DTO
                    │
                    ▼
     [MediatR returns to Controller]
                    │
                    ▼
          return Ok(result) / Created(result)
                    │
                    ▼
          [JSON Serialization]
                    │
                    ▼
          HTTP Response (200/201)
```

---

## Map 4: React Component Tree

```
App.tsx
├── ThemeProvider (ThemeContext)
│   └── ToastProvider (ToastContext)
│       └── AuthProvider (AuthContext)
│           └── Router (React Router v6)
│               ├── /login → LoginPage
│               │   ├── LoginForm (React Hook Form)
│               │   └── useAuth() → AuthContext
│               │
│               ├── /register → RegisterPage
│               │   └── RegisterForm
│               │
│               └── ProtectedRoute (checks AuthContext.userId)
│                   └── /dashboard → DashboardPage
│                       ├── Navbar
│                       │   ├── ThemeToggle (dark/light)
│                       │   ├── UserMenu
│                       │   └── LogoutButton → AuthContext.logout()
│                       │
│                       ├── SearchBar (debounced input)
│                       ├── StatusFilter (ApplicationStatus enum)
│                       ├── ExportCSV button
│                       ├── StaleBanner (stale data warning)
│                       │
│                       ├── AddApplicationModal
│                       │   └── ApplicationForm (React Hook Form)
│                       │
│                       └── ApplicationsList (paginated)
│                           └── JobApplicationCard (× N)
│                               ├── StatusBadge (STATUS_COLORS)
│                               ├── EditButton → EditModal
│                               └── DeleteButton → DELETE /api/...
│
└── ToastContainer (global notifications)
```

---

## Map 5: CI/CD Pipeline

```
Developer pushes to main branch
            │
            ▼
  ┌─────────────────┐
  │  GitHub Actions  │
  │   ci.yml starts  │
  └────────┬────────┘
           │
           ▼
  ┌──────────────────────────────────────────┐
  │  JOB 1: build-and-test                   │
  │  ubuntu-latest                           │
  │  ├── checkout code                       │
  │  ├── setup dotnet 10                     │
  │  ├── dotnet restore                      │
  │  ├── dotnet build --no-restore           │
  │  ├── dotnet test (UnitTests)             │
  │  └── dotnet test (IntegrationTests)      │
  │       Uses: UseEnvironment("Testing")    │
  │             InMemory EF Core             │
  └──────────────┬───────────────────────────┘
                 │ (if success)
                 ▼
  ┌──────────────────────────────────────────┐
  │  JOB 2: docker-build                     │
  │  ├── docker build .                      │
  │  └── (validates Dockerfile, no push)     │
  └──────────────┬───────────────────────────┘
                 │ (runs in parallel with deploy jobs)
        ┌────────┴────────┐
        ▼                 ▼
  ┌─────────────┐  ┌─────────────────────────┐
  │  JOB 3:     │  │  JOB 4:                  │
  │  deploy-api │  │  deploy-frontend          │
  │             │  │                           │
  │  dotnet     │  │  cd frontend              │
  │  publish    │  │  npm ci                   │
  │  -c Release │  │  npm run build            │
  │             │  │  (injects VITE_API_URL    │
  │  Azure      │  │   from GitHub Secret)     │
  │  webapps-   │  │                           │
  │  deploy@v3  │  │  nwtgck/actions-          │
  │             │  │  actions-gh-pages@v4      │
  │  ↓          │  │  deploys dist/            │
  │  Azure App  │  │                           │
  │  Service    │  │  ↓                        │
  │  (Central   │  │  GitHub Pages CDN         │
  │   US)       │  │  ramiro671.github.io      │
  └─────────────┘  └─────────────────────────┘

  Required Secrets:
  AZURE_APP_NAME · AZURE_PUBLISH_PROFILE
  GITHUB_TOKEN (built-in) · VITE_API_URL
```

---

# SECTION 13: FLASHCARDS / Tarjetas de Estudio

---

**Q:** What does REST stand for? / ¿Qué significa REST?
**A:** Representational State Transfer — an architectural style for distributed hypermedia systems. / Transferencia de Estado Representacional — un estilo arquitectónico para sistemas hipermedia distribuidos.

---

**Q:** What is idempotency? / ¿Qué es la idempotencia?
**A:** A property where performing an operation multiple times produces the same result as doing it once. GET, PUT, DELETE are idempotent. POST is not. / Propiedad donde realizar una operación múltiples veces produce el mismo resultado que hacerla una vez.

---

**Q:** What does JWT stand for and what are its 3 parts? / ¿Qué significa JWT y cuáles son sus 3 partes?
**A:** JSON Web Token. Parts: Header (alg + type), Payload (claims), Signature (HMAC of header.payload using secret). / Token Web JSON. Partes: Encabezado, Carga útil (claims), Firma.

---

**Q:** What is the Dependency Inversion Principle? / ¿Qué es el Principio de Inversión de Dependencias?
**A:** High-level modules should not depend on low-level modules. Both should depend on abstractions (interfaces). / Los módulos de alto nivel no deben depender de los de bajo nivel. Ambos deben depender de abstracciones.

---

**Q:** What does CQRS stand for? / ¿Qué significa CQRS?
**A:** Command Query Responsibility Segregation — separating read operations (queries) from write operations (commands). / Segregación de Responsabilidad de Comando y Consulta — separar lecturas de escrituras.

---

**Q:** What is BCrypt? / ¿Qué es BCrypt?
**A:** A password hashing algorithm with built-in salting and a cost factor. Deliberately slow to prevent brute-force attacks. / Un algoritmo de hash de contraseñas con sal incorporada y factor de costo. Deliberadamente lento para prevenir ataques de fuerza bruta.

---

**Q:** What is the Virtual DOM? / ¿Qué es el Virtual DOM?
**A:** An in-memory copy of the real DOM. React diffs the new vs old Virtual DOM and applies minimal updates to the real DOM. / Una copia en memoria del DOM real. React compara el nuevo vs viejo y aplica actualizaciones mínimas al DOM real.

---

**Q:** What does CORS stand for and what does it protect? / ¿Qué significa CORS y qué protege?
**A:** Cross-Origin Resource Sharing. Protects users from malicious sites making authenticated requests to other domains using their browser. / Compartición de Recursos de Origen Cruzado. Protege a usuarios de sitios maliciosos que hacen peticiones autenticadas a otros dominios.

---

**Q:** What is a Pipeline Behavior in MediatR? / ¿Qué es un Pipeline Behavior en MediatR?
**A:** Middleware that wraps every MediatR request/handler, used for cross-cutting concerns like validation, logging, and performance monitoring. / Middleware que envuelve cada petición/handler de MediatR, usado para validación, logging y monitoreo.

---

**Q:** What is the N+1 query problem? / ¿Qué es el problema N+1?
**A:** Loading N records then executing 1 additional query per record = N+1 total queries. Fix with .Include() in EF Core for a single JOIN. / Cargar N registros y ejecutar 1 consulta adicional por registro = N+1 consultas totales. Solución: .Include() para un solo JOIN.

---

**Q:** What does `async/await` do in C#? / ¿Qué hace `async/await` en C#?
**A:** Enables non-blocking I/O. While awaiting a DB/HTTP call, the thread is freed to handle other requests instead of blocking. / Permite I/O no bloqueante. Al hacer await, el hilo se libera para manejar otras peticiones en lugar de bloquear.

---

**Q:** What is the difference between Scoped, Singleton, and Transient DI lifetimes? / ¿Cuál es la diferencia entre Scoped, Singleton y Transient?
**A:** Singleton: one instance for app lifetime. Scoped: one per HTTP request. Transient: new instance every injection. / Singleton: una instancia por toda la app. Scoped: una por petición HTTP. Transient: nueva instancia cada vez.

---

**Q:** What is a SPA? / ¿Qué es una SPA?
**A:** Single Page Application — loads one HTML file, dynamically updates via JavaScript without full page reloads. / Aplicación de Página Única — carga un HTML, actualiza dinámicamente vía JavaScript sin recargas completas.

---

**Q:** What does `useEffect` do? / ¿Qué hace `useEffect`?
**A:** Handles side effects in React functional components: data fetching, subscriptions, DOM manipulation. Runs after render, controlled by dependency array. / Maneja efectos secundarios en componentes funcionales de React. Corre después del render, controlado por el array de dependencias.

---

**Q:** What is Kestrel? / ¿Qué es Kestrel?
**A:** The default cross-platform web server embedded in ASP.NET Core. It handles HTTP connections and passes requests to the middleware pipeline. / El servidor web multiplataforma por defecto embebido en ASP.NET Core.

---

**Q:** What is Redis used for in JobTracker Pro? / ¿Para qué se usa Redis en JobTracker Pro?
**A:** Caching job application lists per user (cache-aside pattern) with TTL. Reduces database load on repeated reads. / Caché de listas de aplicaciones por usuario (patrón cache-aside) con TTL. Reduce la carga de la base de datos en lecturas repetidas.

---

**Q:** What is an EF Core Migration? / ¿Qué es una migración de EF Core?
**A:** A versioned C# file that describes a schema change (add table, add column). Applied in order to bring DB schema in sync with the model. / Un archivo C# versionado que describe un cambio de esquema. Se aplica en orden para sincronizar el esquema de la DB con el modelo.

---

**Q:** What does `[Authorize]` do in ASP.NET Core? / ¿Qué hace `[Authorize]` en ASP.NET Core?
**A:** Requires the request to have a valid authentication (JWT). Returns 401 if no valid token is present. / Requiere que la petición tenga autenticación válida (JWT). Retorna 401 si no hay token válido.

---

**Q:** What is OWASP? / ¿Qué es OWASP?
**A:** Open Web Application Security Project — publishes the OWASP Top 10, the most critical web application security risks. / Proyecto Abierto de Seguridad de Aplicaciones Web — publica el Top 10 de riesgos de seguridad más críticos.

---

**Q:** What is React Context API? / ¿Qué es React Context API?
**A:** A built-in React mechanism for sharing state across components without prop drilling. Used for auth, theme, and toasts in JobTracker Pro. / Mecanismo de React para compartir estado entre componentes sin prop drilling.

---

**Q:** What is TypeScript? / ¿Qué es TypeScript?
**A:** A statically typed superset of JavaScript. Types are erased after compilation — provides compile-time safety only, not runtime. / Un superconjunto de JavaScript con tipado estático. Los tipos se borran tras compilar — solo da seguridad en tiempo de compilación.

---

**Q:** What is the Repository Pattern? / ¿Qué es el patrón Repository?
**A:** An abstraction layer between business logic and data access. Business logic uses an interface; infrastructure implements it. / Una capa de abstracción entre la lógica de negocio y el acceso a datos.

---

**Q:** What is Axios? / ¿Qué es Axios?
**A:** A JavaScript HTTP client library. Key advantages over fetch: interceptors, automatic JSON parsing, throws on 4xx/5xx status codes. / Una librería cliente HTTP de JavaScript. Ventajas sobre fetch: interceptores, parseo JSON automático, lanza errores en 4xx/5xx.

---

**Q:** What is Docker? / ¿Qué es Docker?
**A:** A platform for containerizing applications. A container packages the app + all dependencies into a portable, isolated unit. / Una plataforma para contenerizar aplicaciones. Un contenedor empaqueta la app + todas sus dependencias en una unidad portable y aislada.

---

**Q:** What is CI/CD? / ¿Qué es CI/CD?
**A:** Continuous Integration (auto-build and test on every commit) + Continuous Deployment (auto-deploy to production after tests pass). / Integración Continua (construir y probar automáticamente en cada commit) + Despliegue Continuo (desplegar automáticamente tras pasar pruebas).

---

**Q:** What is the Arrange-Act-Assert pattern? / ¿Qué es el patrón Arrange-Act-Assert?
**A:** A test structure: Arrange (set up dependencies and data), Act (call the method under test), Assert (verify the result). / Estructura de prueba: Arrancar (configurar dependencias), Actuar (llamar el método), Afirmar (verificar el resultado).

---

**Q:** What is a mock in unit testing? / ¿Qué es un mock en pruebas unitarias?
**A:** A fake implementation of a dependency that returns predefined values, allowing isolated testing without real databases or external services. / Una implementación falsa de una dependencia que retorna valores predefinidos, permitiendo pruebas aisladas sin bases de datos reales.

---

**Q:** What is the `sub` claim in a JWT? / ¿Qué es el claim `sub` en un JWT?
**A:** Subject — the unique identifier of the token's subject, typically the user's ID (UUID). Used to identify who made the request. / Sujeto — el identificador único del sujeto del token, típicamente el ID del usuario (UUID).

---

**Q:** What is IDOR? / ¿Qué es IDOR?
**A:** Insecure Direct Object Reference — accessing another user's resource by changing an ID in the URL. Prevented by filtering queries by authenticated userId. / Referencia Directa Insegura a Objetos — acceder a recursos de otro usuario cambiando un ID en la URL.

---

**Q:** What is a Refresh Token? / ¿Qué es un Refresh Token?
**A:** A long-lived token (7 days in JobTracker Pro) stored in the database, used to obtain new access tokens without re-entering credentials. / Un token de larga duración (7 días en JobTracker Pro) almacenado en la base de datos, usado para obtener nuevos access tokens.

---

**Q:** What is `WebApplicationFactory` in .NET testing? / ¿Qué es `WebApplicationFactory` en pruebas .NET?
**A:** A test helper that starts the real ASP.NET Core application in memory, allowing full HTTP pipeline integration tests without a live server. / Un ayudante de pruebas que inicia la app ASP.NET Core real en memoria, permitiendo pruebas de integración completas.

---

**Q:** What does `dotnet publish` do? / ¿Qué hace `dotnet publish`?
**A:** Compiles the app and prepares it for deployment: outputs binaries, dependencies, and config files into a publish folder. Used before Azure deployment. / Compila la app y la prepara para despliegue: genera binarios, dependencias y configuración en una carpeta de publicación.

---

**Q:** What is Neon.tech? / ¿Qué es Neon.tech?
**A:** A serverless PostgreSQL service. Scales to zero when idle, scales automatically on demand. Used in JobTracker Pro instead of Azure PostgreSQL to save ~$60/month. / Un servicio PostgreSQL serverless. Escala a cero cuando está inactivo. Usado en JobTracker Pro en lugar de Azure PostgreSQL para ahorrar ~$60/mes.

---

**Q:** What is Tailwind CSS? / ¿Qué es Tailwind CSS?
**A:** A utility-first CSS framework. Instead of writing custom CSS, apply predefined classes in HTML/JSX (e.g., `bg-blue-500`, `dark:text-white`). / Un framework CSS utility-first. En lugar de CSS personalizado, aplicas clases predefinidas en HTML/JSX.

---

**Q:** What is the Unit of Work pattern? / ¿Qué es el patrón Unit of Work?
**A:** Tracks all changes in a business transaction and commits them as a single atomic unit. EF Core's DbContext is already an implementation. / Rastrea todos los cambios en una transacción y los confirma como una unidad atómica. DbContext de EF Core ya es una implementación.

---

**Q:** What is XSS? / ¿Qué es XSS?
**A:** Cross-Site Scripting — injecting malicious JavaScript into a page to steal data (e.g., JWT from localStorage) or perform actions as the user. / Cross-Site Scripting — inyectar JavaScript malicioso en una página para robar datos o realizar acciones como el usuario.

---

**Q:** What is CSRF? / ¿Qué es CSRF?
**A:** Cross-Site Request Forgery — tricking a browser into making authenticated requests using stored cookies. Not applicable when using localStorage for JWT. / Cross-Site Request Forgery — engañar al navegador para que haga peticiones autenticadas usando cookies almacenadas.

---

**Q:** What is the `as const` assertion in TypeScript? / ¿Qué es `as const` en TypeScript?
**A:** Makes all values in an object literal types instead of broadened types. `Status.Applied` becomes type `1` instead of `number`. / Hace que todos los valores sean tipos literales en lugar de tipos ampliados.

---

**Q:** What does `dotnet ef migrations add` do? / ¿Qué hace `dotnet ef migrations add`?
**A:** Generates a new migration file by comparing the current EF Core model snapshot with the previous one, outputting the C# code for schema changes. / Genera un nuevo archivo de migración comparando el snapshot del modelo actual con el anterior.

---

---

# SECTION 14: PRACTICE EXAM / Examen de Práctica

---

## Part 1: Multiple Choice / Parte 1: Opción Múltiple

**1.** Which HTTP status code should be returned when a user sends a valid JWT but tries to access another user's data?

- A) 401 Unauthorized
- B) 400 Bad Request
- **C) 403 Forbidden** ✓
- D) 404 Not Found

---

**2.** In Clean Architecture, which layer is allowed to have zero external NuGet dependencies?

- A) API Layer
- B) Infrastructure Layer
- C) Application Layer
- **D) Domain Layer** ✓

---

**3.** Which CQRS concept is used to change state (write to the database)?

- A) Query
- **B) Command** ✓
- C) Handler
- D) Behavior

---

**4.** What is the primary purpose of the `ValidationBehavior` Pipeline Behavior in JobTracker Pro?

- A) Log all requests and responses
- B) Cache frequently accessed data
- **C) Automatically validate all Commands before they reach the handler** ✓
- D) Manage database transactions

---

**5.** Why does JobTracker Pro return `Status` as `int` in the DTO instead of a string?

- A) Strings take more bandwidth
- B) PostgreSQL doesn't support enum serialization
- **C) The frontend compares `a.status === filter` where filter is a number** ✓
- D) JSON doesn't support string enums

---

**6.** What is the correct CORS behavior when a browser makes a non-simple cross-origin request?

- A) The browser blocks the request immediately
- B) The server rejects without any notification
- **C) The browser sends an OPTIONS preflight request first** ✓
- D) The request goes through with a warning header

---

**7.** Which DI lifetime creates a new instance of a service every single time it is requested?

- A) Scoped
- B) Singleton
- **C) Transient** ✓
- D) PerRequest

---

**8.** In JobTracker Pro's CI/CD pipeline, what triggers the GitHub Actions workflow?

- A) Pull requests to any branch
- **B) Push or PR to the `main` branch** ✓
- C) Manual trigger only
- D) Any branch push

---

**9.** What does BCrypt's "cost factor" control?

- A) The length of the generated hash
- B) The number of salt rounds stored
- **C) How computationally expensive (slow) the hashing is** ✓
- D) The encryption algorithm used

---

**10.** Why is `IsEnvironment("Testing")` used instead of `IsRelational()` to guard migrations in Program.cs?

- A) IsRelational() is deprecated in .NET 10
- **B) IsRelational() returns true even for InMemory provider in certain configurations** ✓
- C) IsEnvironment() is faster to execute
- D) InMemory database supports migrations in newer EF Core versions

---

## Part 2: True / False / Parte 2: Verdadero / Falso

**11.** REST APIs are stateless — the server stores session information between requests.
**FALSE** ✗ — REST is stateless. No session is stored on the server. Each request must contain all needed information (JWT contains user identity).

---

**12.** CORS protects the server from unauthorized API calls made with curl or Postman.
**FALSE** ✗ — CORS only applies to browser-based requests. curl and Postman ignore CORS entirely. Server-side authentication (JWT) is what actually protects the API.

---

**13.** A JWT's payload is encrypted by default.
**FALSE** ✗ — JWT payload is Base64URL encoded, not encrypted. Anyone can decode it. Never put sensitive data (passwords, credit cards) in JWT claims. Use JWE for encryption.

---

**14.** In Clean Architecture, the Application layer can directly reference Entity Framework Core.
**FALSE** ✗ — The Application layer can only depend on the Domain layer. EF Core lives in the Infrastructure layer. The Application layer uses repository interfaces (defined in Domain) and doesn't know about EF Core.

---

**15.** `useEffect` with an empty dependency array `[]` runs after every render.
**FALSE** ✗ — It runs only once after the initial mount (component first appears in DOM). Equivalent to `componentDidMount` in class components.

---

**16.** The Repository Pattern in Clean Architecture allows swapping PostgreSQL for a different database without modifying the Application layer.
**TRUE** ✓ — Because Application depends on `IJobApplicationRepository` (interface), not the EF Core implementation. A new implementation can be created and swapped in the DI container.

---

**17.** Storing JWT in localStorage is vulnerable to CSRF attacks.
**FALSE** ✗ — localStorage is NOT automatically sent with requests (unlike cookies). XSS is the risk with localStorage. CSRF applies to cookie-based auth.

---

**18.** Integration tests in JobTracker Pro use a real PostgreSQL database.
**FALSE** ✗ — They use InMemory EF Core database. The `CustomWebApplicationFactory` replaces the real DbContext with `UseInMemoryDatabase`. This is why migration guard uses `IsEnvironment("Testing")`.

---

**19.** In CQRS, a Query should have side effects on the database.
**FALSE** ✗ — Queries are read-only operations. They should not modify state. Only Commands change state. This is the fundamental principle of CQRS.

---

**20.** Making the jobtracker-pro GitHub repository public exposes production secrets if they were stored in Azure App Service Configuration.
**FALSE** ✗ — Azure App Service Configuration is private and not stored in the Git repository. The repository only contains code. Secrets accessed by `Environment.GetEnvironmentVariable()` or the Azure configuration API are never in the codebase.

---

## Part 3: Short Answer / Parte 3: Respuesta Corta

**21.** Explain the difference between `401 Unauthorized` and `403 Forbidden`. Give an example from JobTracker Pro.

**Expected answer:** 401 means the request has no valid authentication — missing JWT or invalid/expired JWT. 403 means the user IS authenticated but lacks permission. In JobTracker Pro: calling `GET /api/jobapplications` without a JWT → 401. Calling it with User A's valid JWT while trying to access User B's application ID → 403 (or 404 for security).

---

**22.** What is the purpose of the Refresh Token in the authentication flow?

**Expected answer:** The access token (JWT) is short-lived (60 min) to limit exposure if stolen. The refresh token is long-lived (7 days) and stored in the database. When the access token expires, the client sends the refresh token to get a new access token without requiring the user to log in again. Refresh tokens can be individually revoked from the database.

---

**23.** Why does the middleware order in ASP.NET Core matter? Give a concrete example.

**Expected answer:** Middleware is a pipeline — each component runs in order. If `UseAuthentication()` comes after `UseAuthorization()`, authorization checks run before the user identity is established, causing all authorized endpoints to fail with 401. Similarly, `UseCors()` must come before routing to ensure CORS headers are set before the request reaches controllers.

---

**24.** What is the N+1 query problem and how does EF Core's `.Include()` solve it?

**Expected answer:** N+1 occurs when loading N parent entities and then making one database query per entity to load related children — resulting in 1 + N queries total. `.Include()` instructs EF Core to use a SQL JOIN in a single query, loading parents and their related children together. Without it, loading 50 users each with their job applications would generate 51 queries instead of 1.

---

**25.** Explain why the frontend's `ApplicationStatus` values intentionally differ from the backend's enum values, and why this is acceptable.

**Expected answer:** The frontend `ApplicationStatus` const and the backend `ApplicationStatus` enum were developed independently. The frontend uses its own numeric values for display logic and filtering. The DTO returns `status` as `int`. As long as both sides are consistent in their own context and the int value flows correctly from DB → DTO → frontend, there's no functional issue. Changing either side requires updating and testing both simultaneously.

---

**26.** What is a Pipeline Behavior in MediatR and what problem does it solve?

**Expected answer:** A Pipeline Behavior wraps every MediatR request, running logic before and/or after the handler. It solves the cross-cutting concern problem: instead of adding validation, logging, or error handling code to every handler individually, you write it once as a behavior and it applies automatically. In JobTracker Pro, `ValidationBehavior` runs FluentValidation on all Commands before they reach the handler.

---

**27.** Why is Neon.tech used instead of Azure Database for PostgreSQL in JobTracker Pro?

**Expected answer:** Azure Database for PostgreSQL costs approximately $60/month. Neon.tech's free tier provides serverless PostgreSQL that scales to zero when idle, making it suitable for a portfolio project with low traffic. The tradeoff is potential cold-start latency after periods of inactivity. The connection string format uses Npgsql syntax with SSL Mode=Require.

---

**28.** Describe the Axios interceptor's queue pattern for handling concurrent requests during token refresh.

**Expected answer:** When the first 401 is caught, `isRefreshing` is set to true and the request is retried after refresh. Subsequent concurrent requests that also get 401 are not immediately retried — they're added to a `failedQueue` as Promises. When refresh completes, `processQueue()` resolves all queued promises with the new token, and each queued request is retried. This prevents multiple simultaneous refresh calls (race condition) and ensures all requests eventually complete.

---

**29.** What is the Dependency Inversion Principle and how does it enable unit testing in JobTracker Pro?

**Expected answer:** DIP states that high-level modules should depend on abstractions, not concrete implementations. `CreateJobApplicationHandler` depends on `IJobApplicationRepository` (interface), not the EF Core `JobApplicationRepository`. In unit tests, we inject a `Mock<IJobApplicationRepository>` that returns predefined data without any database connection. Without this, every unit test would require a running PostgreSQL instance.

---

**30.** Explain how SPA routing works on GitHub Pages in the JobTracker Pro project.

**Expected answer:** GitHub Pages serves a `404.html` file when a path doesn't match a physical file. The project places a `404.html` in `frontend/public/` (Vite copies it to `dist/`) that captures the URL path, stores it in `sessionStorage`, and redirects to `index.html`. The React app then reads from `sessionStorage` on startup and restores the intended route. `BrowserRouter` is configured with `basename="/jobtracker-pro"` to match the GitHub Pages sub-path `https://ramiro671.github.io/jobtracker-pro/`.

---

## ANSWER KEY / CLAVE DE RESPUESTAS

| # | Type | Answer |
|---|------|--------|
| 1 | MC | C |
| 2 | MC | D |
| 3 | MC | B |
| 4 | MC | C |
| 5 | MC | C |
| 6 | MC | C |
| 7 | MC | C |
| 8 | MC | B |
| 9 | MC | C |
| 10 | MC | B |
| 11 | T/F | FALSE |
| 12 | T/F | FALSE |
| 13 | T/F | FALSE |
| 14 | T/F | FALSE |
| 15 | T/F | FALSE |
| 16 | T/F | TRUE |
| 17 | T/F | FALSE |
| 18 | T/F | FALSE |
| 19 | T/F | FALSE |
| 20 | T/F | FALSE |
| 21-30 | Short | See detailed answers above |

---

## SCORING / PUNTUACIÓN

| Score | Level |
|-------|-------|
| 28-30 | Senior-ready 🚀 |
| 24-27 | Mid-level strong 💪 |
| 18-23 | Mid-level, review weak areas 📚 |
| <18 | Review sections 1-6 first 🔄 |

---

*JobTracker Pro Study Guide — Generated March 2026*
*Ramiro López — Senior .NET Developer Portfolio*
