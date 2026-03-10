# JobTracker Pro

A full-stack web application to track job applications, built with **ASP.NET Core 8**, **Clean Architecture**, and **React + TypeScript**.

> Built as a portfolio project to demonstrate Senior .NET Developer skills: Clean Architecture, CQRS, JWT authentication, Docker, CI/CD, and full-stack development.

---

## Live Demo

| Service | URL |
|---------|-----|
| API (Swagger) | `http://localhost:5086` |
| Frontend | _coming soon_ |

---

## Architecture

This project follows **Clean Architecture** with strict dependency rules — dependencies always point inward toward the Domain layer.

```
┌─────────────────────────────────────────────────┐
│                    API Layer                    │
│         Controllers · Middleware · DI           │
└───────────────────┬─────────────────────────────┘
                    │
        ┌───────────┴────────────┐
        ▼                        ▼
┌──────────────────┐   ┌──────────────────────┐
│ Application Layer│   │ Infrastructure Layer │
│  MediatR · CQRS  │   │  EF Core · JWT       │
│  FluentValidation│   │  Repositories · BCrypt│
│  Knows: Domain   │   │  Knows: Domain + App  │
└────────┬─────────┘   └──────────────────────┘
         │
         ▼
┌──────────────────┐
│   Domain Layer   │
│ Entities · Enums │
│ Value Objects    │
│ Interfaces       │
│ Knows: nobody    │
└──────────────────┘
```

### Layer responsibilities

| Layer | Responsibility |
|-------|---------------|
| **Domain** | Entities, enums, value objects, repository interfaces. No external dependencies. |
| **Application** | Use cases via MediatR CQRS. Commands + Queries + Handlers + DTOs. Input validation with FluentValidation. |
| **Infrastructure** | EF Core + PostgreSQL, repository implementations, JWT generation, BCrypt password hashing, refresh tokens. |
| **API** | HTTP controllers, Swagger/OpenAPI, global error handling middleware, DI wiring. |

---

## Tech Stack

### Backend
| Technology | Purpose |
|-----------|---------|
| ASP.NET Core 8 | Web API framework |
| C# 13 / .NET 10 | Language and runtime |
| Entity Framework Core | ORM and migrations |
| PostgreSQL 16 | Primary database |
| MediatR | CQRS pattern dispatcher |
| FluentValidation | Input validation pipeline |
| JWT Bearer | Stateless authentication |
| BCrypt.Net | Password hashing |
| Serilog | Structured logging |
| Docker + Docker Compose | Local development environment |

### Frontend _(in progress)_
| Technology | Purpose |
|-----------|---------|
| React 18 + TypeScript | UI framework |
| Vite | Build tool |
| Tailwind CSS | Styling |
| React Hook Form | Form management |
| React Router | Client-side routing |
| Axios | HTTP client |

### Testing _(in progress)_
| Technology | Purpose |
|-----------|---------|
| xUnit | Unit test framework |
| Moq | Mocking library |
| FluentAssertions | Readable assertions |
| WebApplicationFactory | Integration testing |

### DevOps _(in progress)_
| Technology | Purpose |
|-----------|---------|
| GitHub Actions | CI/CD pipeline |
| Docker | Containerization |
| Azure App Service | Backend hosting |
| Netlify | Frontend hosting |

---

## Project Structure

```
JobTrackerPro.sln
├── src/
│   ├── JobTrackerPro.Domain/
│   │   ├── Entities/             # JobApplication, Company, User, RefreshToken
│   │   ├── Enums/                # ApplicationStatus, SeniorityLevel, WorkModality
│   │   ├── Interfaces/           # IJobApplicationRepository, IUnitOfWork, etc.
│   │   └── ValueObjects/         # TechStack, DateRange
│   │
│   ├── JobTrackerPro.Application/
│   │   ├── Auth/Commands/        # Register, Login, RefreshToken handlers
│   │   ├── JobApplications/      # CRUD commands and queries
│   │   ├── Common/Behaviors/     # ValidationBehavior (MediatR pipeline)
│   │   ├── Common/Interfaces/    # IJwtTokenGenerator
│   │   └── DTOs/                 # JobApplicationDto, AuthResponse, etc.
│   │
│   ├── JobTrackerPro.Infrastructure/
│   │   ├── Persistence/          # ApplicationDbContext, Repositories, UnitOfWork
│   │   ├── Authentication/       # JwtTokenGenerator, JwtSettings
│   │   └── DependencyInjection.cs
│   │
│   └── JobTrackerPro.Api/
│       ├── Controllers/          # AuthController, JobApplicationsController
│       ├── Middleware/           # ExceptionHandlingMiddleware
│       └── Program.cs
│
└── tests/
    ├── JobTrackerPro.UnitTests/
    └── JobTrackerPro.IntegrationTests/
```

---

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [Node.js v20+](https://nodejs.org/) _(for frontend)_

### 1. Clone the repository

```bash
git clone https://github.com/Ramiro671/jobtracker-pro.git
cd jobtracker-pro
```

### 2. Start PostgreSQL with Docker

```bash
docker-compose up -d
```

This starts PostgreSQL 16 on `localhost:5432` with database `jobtracker_dev`.

### 3. Apply database migrations

```bash
dotnet ef database update \
  --project src/JobTrackerPro.Infrastructure \
  --startup-project src/JobTrackerPro.Api
```

### 4. Run the API

```bash
dotnet run --project src/JobTrackerPro.Api
```

Open Swagger UI at: **http://localhost:5086**

---

## API Endpoints

### Authentication

| Method | Endpoint | Description | Auth required |
|--------|----------|-------------|---------------|
| `POST` | `/api/auth/register` | Register a new user | ❌ |
| `POST` | `/api/auth/login` | Login and receive tokens | ❌ |
| `POST` | `/api/auth/refresh` | Refresh access token | ❌ |

**Register / Login response:**
```json
{
  "accessToken": "eyJhbGci...",
  "refreshToken": "oF7/8BEy...",
  "expiresAt": "2026-03-10T21:48:53Z"
}
```

### Job Applications

All endpoints require `Authorization: Bearer {accessToken}` header.

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/jobapplications/{userId}` | Get all applications for a user |
| `POST` | `/api/jobapplications` | Create a new application |
| `PUT` | `/api/jobapplications/{id}` | Update application status |
| `DELETE` | `/api/jobapplications/{id}` | Delete an application |

**Create application request:**
```json
{
  "userId": "11111111-1111-1111-1111-111111111111",
  "title": "Senior .NET Developer",
  "companyName": "Anthropic",
  "jobUrl": "https://anthropic.com/careers/123",
  "description": "Remote position, ASP.NET Core + React",
  "source": "LinkedIn"
}
```

**Update status request:**
```json
{
  "newStatus": 2,
  "notes": "Technical interview scheduled for next week"
}
```

**Application status values:**

| Value | Status |
|-------|--------|
| 0 | Applied |
| 1 | PhoneScreen |
| 2 | Interview |
| 3 | TechnicalTest |
| 4 | FinalInterview |
| 5 | OfferReceived |
| 6 | OfferAccepted |
| 7 | Rejected |
| 8 | Withdrawn |

---

## Authentication Flow

```
1. POST /api/auth/register → { accessToken (60 min), refreshToken (7 days) }
2. Use accessToken in every request: Authorization: Bearer eyJhbGci...
3. When accessToken expires → POST /api/auth/refresh { token: "refreshToken" }
4. Receive new accessToken + new refreshToken (token rotation)
```

---

## Configuration

All secrets are managed via environment variables or `appsettings.json`.
**Never commit real secrets to source control.**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=jobtracker_dev;Username=postgres;Password=postgres"
  },
  "JwtSettings": {
    "Secret": "your-secret-key-here",
    "Issuer": "JobTrackerPro",
    "Audience": "JobTrackerPro",
    "ExpirationMinutes": 60
  }
}
```

---

## Development Progress

| Phase | Period | Content | Status |
|-------|--------|---------|--------|
| Phase 1 | Mar 5–31 | ASP.NET Core · EF Core · JWT · Clean Architecture | 🔄 In progress |
| Phase 2 | Apr 1–21 | xUnit · Docker · Redis · CI/CD | ⏳ |
| Phase 3 | Apr 22+ | React · TypeScript · Auth UI | ⏳ |

### Completed blocks

| Block | Content |
|-------|---------|
| ✅ 1 | Clean Architecture solution setup |
| ✅ 2-3 | Domain layer: entities, value objects, interfaces |
| ✅ 4 | Application layer: MediatR CQRS + DTOs |
| ✅ 5 | Infrastructure: repositories + unit of work |
| ✅ 6 | API: controller + Swagger UI |
| ✅ 7 | Docker + PostgreSQL + EF Core migrations |
| ✅ 8 | Repository pattern: full implementation |
| ✅ 9 | CRUD: PUT + DELETE endpoints |
| ✅ 10 | FluentValidation + global error handler |
| ✅ 11 | JWT authentication: register + login |
| ✅ 12 | Refresh tokens + [Authorize] + Swagger Bearer |
| ✅ 13 | Clean Architecture audit + refactor |
| ✅ 14 | Serilog structured logging |

---

## License

MIT License — see [LICENSE](LICENSE) for details.

---

_Built by [Ramiro López](https://github.com/Ramiro671) · March 2026_
