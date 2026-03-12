# JobTracker Pro

![CI/CD](https://github.com/Ramiro671/jobtracker-pro/actions/workflows/ci.yml/badge.svg)
![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)
![React](https://img.shields.io/badge/React-18-61DAFB)
![License](https://img.shields.io/badge/license-MIT-green)

A full-stack web application to track job applications, built with **ASP.NET Core 10**, **Clean Architecture**, and **React + TypeScript**.

> Built as a portfolio project demonstrating Senior .NET Developer skills: Clean Architecture, CQRS, JWT authentication, Redis caching, Docker, CI/CD, and full-stack development.

---

## Features

- ✅ **Track applications** — create, update status, delete job applications
- ✅ **JWT authentication** — register, login, refresh tokens with rotation
- ✅ **Real-time stats** — total, active, and offer counts on dashboard
- ✅ **Status filtering** — filter by any of 9 application stages
- ✅ **Redis caching** — fast reads with cache-aside pattern
- ✅ **Structured logging** — Serilog with rolling file output
- ✅ **Full test coverage** — 16 tests (unit + integration)
- ✅ **Containerized** — Docker multi-stage build, Docker Compose stack
- ✅ **CI/CD** — GitHub Actions pipeline with Azure + Netlify deploy

---

## Architecture

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
│  FluentValidation│   │  Redis · BCrypt      │
│  Knows: Domain   │   │  Knows: Domain + App │
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

---

## Tech Stack

### Backend
| Technology | Purpose |
|-----------|---------|
| ASP.NET Core 10 | Web API framework |
| C# / .NET 10 | Language and runtime |
| Entity Framework Core | ORM and migrations |
| PostgreSQL 16 | Primary database |
| MediatR | CQRS pattern |
| FluentValidation | Input validation pipeline |
| JWT Bearer | Stateless authentication |
| BCrypt.Net | Password hashing |
| Redis | Distributed caching |
| Serilog | Structured logging |

### Frontend
| Technology | Purpose |
|-----------|---------|
| React 18 + TypeScript | UI framework |
| Vite | Build tool |
| Tailwind CSS | Styling |
| Axios | HTTP client + interceptors |
| React Router | Client-side routing |

### Testing
| Technology | Purpose |
|-----------|---------|
| xUnit | Unit test framework |
| Moq | Mocking library |
| FluentAssertions | Readable assertions |
| WebApplicationFactory | Integration testing |

### DevOps
| Technology | Purpose |
|-----------|---------|
| Docker + Compose | Containerization |
| GitHub Actions | CI/CD pipeline |
| Azure App Service | Backend hosting |
| Netlify | Frontend hosting |
| Application Insights | Monitoring |

---

## Getting Started

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [Node.js 22+](https://nodejs.org/)

### 1. Clone
```bash
git clone https://github.com/Ramiro671/jobtracker-pro.git
cd jobtracker-pro
```

### 2. Start infrastructure
```bash
docker-compose up postgres redis -d
```

### 3. Apply migrations
```bash
dotnet ef database update \
  --project src/JobTrackerPro.Infrastructure \
  --startup-project src/JobTrackerPro.Api
```

### 4. Run API
```bash
dotnet run --project src/JobTrackerPro.Api
# → http://localhost:5086 (Swagger UI)
```

### 5. Run frontend
```bash
cd frontend
npm install
npm run dev
# → http://localhost:5173
```

### 6. Run tests
```bash
dotnet test
```

---

## API Reference

### Auth endpoints (public)

| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/api/auth/register` | Register + receive tokens |
| `POST` | `/api/auth/login` | Login + receive tokens |
| `POST` | `/api/auth/refresh` | Refresh access token |

### Job Applications (requires Bearer token)

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/jobapplications/{userId}` | Get all applications |
| `POST` | `/api/jobapplications` | Create application |
| `PUT` | `/api/jobapplications/{id}` | Update status |
| `DELETE` | `/api/jobapplications/{id}` | Delete application |

### Application Status values

| Value | Status |
|-------|--------|
| 0 | Applied |
| 1 | Phone Screen |
| 2 | Interview |
| 3 | Technical Test |
| 4 | Final Interview |
| 5 | Offer Received |
| 6 | Offer Accepted |
| 7 | Rejected |
| 8 | Withdrawn |

---

## Testing

```bash
# All tests
dotnet test

# Unit tests only
dotnet test tests/JobTrackerPro.UnitTests

# Integration tests only
dotnet test tests/JobTrackerPro.IntegrationTests
```

**16 tests total — 0 failures.**

---

## Project Structure

```
JobTrackerPro.sln
├── src/
│   ├── JobTrackerPro.Domain/          # Entities, enums, value objects, interfaces
│   ├── JobTrackerPro.Application/     # Use cases, CQRS, validators, DTOs
│   ├── JobTrackerPro.Infrastructure/  # EF Core, JWT, Redis, repositories
│   └── JobTrackerPro.Api/             # Controllers, middleware, Swagger
├── tests/
│   ├── JobTrackerPro.UnitTests/       # Handler tests with Moq
│   └── JobTrackerPro.IntegrationTests/ # HTTP tests with WebApplicationFactory
├── frontend/                          # React + TypeScript + Tailwind
├── docs/daily-logs/                   # Development log entries
├── Dockerfile                         # Multi-stage build
├── docker-compose.yml                 # Local dev stack
└── .github/workflows/ci.yml          # CI/CD pipeline
```

---

## Version History

See [CHANGELOG.md](CHANGELOG.md) for full release history.

Current version: **v1.0.0** (March 11, 2026)

---

## License

MIT License — see [LICENSE](LICENSE) for details.

---

_Built by [Ramiro López](https://github.com/Ramiro671) · March 2026_
