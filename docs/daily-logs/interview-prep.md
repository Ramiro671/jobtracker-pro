# Interview Preparation — JobTracker Pro

---

## BLOQUE 31 — How to describe the project (60 seconds)

### Elevator pitch — memorize this

"I built JobTracker Pro, a full-stack web application to track job applications.
The backend is ASP.NET Core 10 with Clean Architecture and CQRS using MediatR.
It uses PostgreSQL for persistence, Redis for caching, and JWT with refresh token rotation for authentication.
The frontend is React with TypeScript and Tailwind CSS.
I containerized it with Docker, set up a CI/CD pipeline with GitHub Actions,
and deployed the API to Azure App Service and the frontend to GitHub Pages.
The project has 26 automated tests — 10 unit tests with Moq and 16 integration tests
with WebApplicationFactory. I built it in 3 weeks to demonstrate senior-level patterns."

---

### Extended version — for deeper questions

**Problem it solves:**
"When applying to many companies, it's easy to lose track of where you are in
each process. JobTracker Pro gives you a single dashboard to see all applications,
their current status, and key dates."

**Technical decisions:**
- "I chose Clean Architecture to enforce strict dependency boundaries.
  The domain has zero external dependencies — it's pure C#."
- "I used CQRS with MediatR to separate reads from writes and keep handlers small.
  Each handler does exactly one thing."
- "For caching I implemented the cache-aside pattern — check Redis first,
  fall back to PostgreSQL on a miss, then populate the cache."
- "JWT with refresh token rotation: access tokens expire in 60 minutes,
  refresh tokens in 7 days. On every refresh, the old token is revoked
  and a new one is issued — this detects token theft."

**Challenges faced:**
- "The biggest challenge was keeping Application independent from Infrastructure.
  I had IJwtTokenGenerator in the wrong layer — I caught it during a code audit
  and moved the interface to Application so handlers don't reference Infrastructure."

---

## BLOQUE 32 — 20 Technical Interview Questions

---

### Clean Architecture

**Q1: What is Clean Architecture and why did you use it?**
Clean Architecture organizes code in concentric layers where dependencies
always point inward toward the domain. I used it because it makes the code
testable — handlers don't depend on databases or HTTP, so I can test them
with mocks. It also makes the codebase maintainable: changing the database
from PostgreSQL to SQL Server only touches Infrastructure, not Application or Domain.

**Q2: What is the Dependency Inversion Principle? Give an example from your project.**
High-level modules should not depend on low-level modules — both should depend
on abstractions. In my project, `RegisterHandler` (Application) needs to generate
a JWT token. Instead of depending on `JwtTokenGenerator` (Infrastructure) directly,
it depends on `IJwtTokenGenerator` (an interface in Application). Infrastructure
provides the implementation. Application never imports Infrastructure.

**Q3: What is the difference between Domain and Application layers?**
Domain contains the business entities, enums, value objects, and repository
interfaces — it's pure business logic with zero external dependencies.
Application contains the use cases: commands, queries, handlers, validators,
and DTOs. Application knows about Domain but not about how data is persisted
or how tokens are generated — those are Infrastructure concerns.

---

### CQRS + MediatR

**Q4: What is CQRS and what problem does it solve?**
Command Query Responsibility Segregation separates operations that change
state (commands) from operations that read state (queries). It solves the
problem of mixed concerns in a single service class. In my project,
`CreateJobApplicationCommand` handles writes and `GetJobApplicationsQuery`
handles reads. Each has its own handler with a single responsibility.

**Q5: What is a MediatR Pipeline Behavior?**
A pipeline behavior wraps every request before it reaches the handler — like
middleware for MediatR. I used `ValidationBehavior<TRequest, TResponse>` to
run FluentValidation validators on every command automatically. If validation
fails, a `ValidationException` is thrown before the handler even executes.
No need for try/catch in every controller.

---

### JWT Authentication

**Q6: How does JWT authentication work in your project?**
The client sends POST /api/auth/login with email and password. The server
verifies the BCrypt hash, then generates a signed JWT with claims (userId,
email, expiration). The client stores the token and sends it as
`Authorization: Bearer {token}` on every request. ASP.NET Core validates
the signature on each request — no database lookup needed.

**Q7: What is refresh token rotation and why is it secure?**
Access tokens expire in 60 minutes. When they expire, the client sends
the refresh token to POST /api/auth/refresh. The server validates it,
immediately revokes it (marks IsRevoked=true), creates a new refresh token,
and returns both tokens. If an attacker steals a refresh token and tries
to use it after the legitimate user already refreshed — it's already revoked.
This detects token theft.

**Q8: Why not store JWT tokens in localStorage?**
localStorage is vulnerable to XSS attacks — any injected script can read it.
For this portfolio project I used localStorage for simplicity, but in production
I would use HttpOnly cookies, which JavaScript cannot access at all.

---

### Entity Framework Core

**Q9: What is the Repository Pattern and why use it?**
The repository pattern abstracts data access behind an interface.
`IJobApplicationRepository` defines what operations are available —
the handler doesn't know if the data comes from PostgreSQL, SQL Server,
or an in-memory database. This is what makes integration tests possible:
I swap the real PostgreSQL for EF Core InMemory in tests without changing
any application code.

**Q10: What is the Unit of Work pattern?**
Unit of Work groups multiple repository operations into a single transaction.
Without it, each repository would call SaveChanges independently, which could
leave the database in an inconsistent state. `IUnitOfWork.SaveChangesAsync()`
is called once at the end of each command handler, committing all changes atomically.

---

### Testing

**Q11: What is the difference between unit tests and integration tests?**
Unit tests test a single class in isolation — dependencies are mocked with Moq.
They're fast (milliseconds) and test logic bugs.
Integration tests test the full HTTP pipeline: real middleware, real routing,
real EF Core (InMemory), real validation. They're slower but catch wiring bugs
that unit tests miss. I have 10 unit tests and 16 integration tests — 26 total.

**Q12: How does WebApplicationFactory work?**
WebApplicationFactory spins up the full ASP.NET Core application in memory
for testing. I override `ConfigureWebHost` to replace PostgreSQL with
EF Core InMemory. The test sends real HTTP requests with `HttpClient` and
receives real HTTP responses — same as Postman but automated and repeatable.

**Q13: How do you test that an endpoint returns 401 without a token?**
```csharp
var response = await Client.GetAsync("/api/jobapplications/{userId}");
response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
```
No token in the request → ASP.NET Core authentication middleware rejects it
before reaching the controller → 401. This verifies [Authorize] is working.

---

### Redis + Caching

**Q14: What is the cache-aside pattern?**
Also called lazy loading: check the cache first. On a hit, return cached data.
On a miss, query the database, store the result in cache, then return it.
I cache GET /api/jobapplications/{userId} for 10 minutes. On POST/DELETE,
I invalidate the cache key so the next GET reflects the latest data.

---

### Docker

**Q15: Why use a multi-stage Dockerfile?**
The build stage uses the .NET SDK image (~750MB) to compile and publish.
The runtime stage uses only the ASP.NET runtime image (~210MB before app).
The final image is ~106MB because it only contains the runtime and published
output — no SDK, no source code, no build tools. Smaller images mean faster
deploys and smaller attack surface.

---

### FluentValidation

**Q16: Why validate in the Application layer instead of the controller?**
Controllers are infrastructure (HTTP concerns). Validation is a business rule —
if a job title is required, that's an application concern, not an HTTP concern.
Using a MediatR pipeline behavior, validation runs automatically on every
command before the handler executes. The controller doesn't need any
validation code at all — it just passes the request to MediatR.

---

### General Senior Questions

**Q17: What SOLID principles did you apply?**
- **S** (SRP): Each handler does one thing. Each validator validates one command.
- **O** (OCP): Adding a new use case means adding a new handler — existing handlers unchanged.
- **L** (LSP): All repositories implement their interfaces and are interchangeable.
- **I** (ISP): `IJobApplicationRepository` only has methods relevant to job applications.
- **D** (DIP): Handlers depend on interfaces, not concrete implementations.

**Q18: How would you scale this application?**
"Currently it's a monolith — appropriate for this stage.
To scale: add read replicas for PostgreSQL, increase Redis TTL for hot data,
containerize with Kubernetes for horizontal scaling, add an API Gateway for
rate limiting, and consider splitting into microservices only if a specific
bounded context has significantly different scaling needs."

**Q19: What would you improve if you had more time?**
"First, email notifications for status changes using a background job with
Hangfire or Azure Service Bus. Second, proper role-based authorization.
Third, a refresh token family invalidation strategy to invalidate all tokens
when theft is detected. Fourth, end-to-end tests with Playwright."

**Q20: How do you handle secrets in production?**
"In development, secrets go in appsettings.json (not committed) or user secrets.
In production, they go in Azure App Service Configuration (environment variables)
or Azure Key Vault. The application reads them via IConfiguration — the code
never changes between environments. The CI/CD pipeline injects production secrets
as GitHub Actions secrets, which are never logged or exposed in output."

---

## Quick reference — numbers to remember

| Metric | Value |
|--------|-------|
| Layers | 4 (Domain, Application, Infrastructure, API) |
| Endpoints | 7 (3 auth + 4 CRUD) |
| Unit tests | 10 |
| Integration tests | 16 |
| Docker image size | 106 MB |
| JWT expiry | 60 minutes |
| Refresh token expiry | 7 days |
| Cache TTL | 10 minutes |
| ApplicationStatus stages | 9 |
| Build time (CI) | ~45 seconds |
