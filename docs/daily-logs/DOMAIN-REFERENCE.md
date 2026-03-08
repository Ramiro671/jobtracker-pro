# Domain Layer — Entity Design Reference

## Evolution: LinkedInAgent.Grpc → JobTracker Pro

### What changed and why

| LinkedInAgent (old)        | JobTracker Pro (new)         | Why                                         |
|----------------------------|------------------------------|---------------------------------------------|
| `string Status = "Crudo"`  | `enum ApplicationStatus`     | Type safety, no magic strings               |
| `string SeniorityLevel`    | `enum SeniorityLevel`        | Consistent values across the app            |
| `string Technologies`      | `TechStack` value object     | Categorized (Back/Front/DB/Cloud/Testing)   |
| `MongoDbService` (god class) | `IJobApplicationRepository` | Single responsibility + interface           |
| Hardcoded connection string | `appsettings.json` + env vars | Security best practice                     |
| Spanish method names       | English everywhere            | International job readiness                 |
| No User concept            | `User` entity + `UserId` FK  | Multi-tenant, JWT-ready                     |
| Flat `JobOffer` model      | `JobApplication` aggregate   | Rich domain model with behavior methods     |

### Gold Layer Alignment

The `TechStack` value object categories map directly to the Gold layer analysis:

```
Gold Layer Output          →  TechStack Property
─────────────────────────────────────────────────
Backend: C#/.NET Core...   →  TechStack.Backend
Frontend: React, Angular.. →  TechStack.Frontend
DB: SQL Server, Postgres.. →  TechStack.Databases
Cloud: Azure, Docker...    →  TechStack.CloudAndDevOps
Testing: xUnit, NUnit...   →  TechStack.Testing
```

### Domain Structure

```
JobTrackerPro.Domain/
├── Entities/
│   ├── JobApplication.cs    ← Aggregate root (was JobOffer + JobOfferSilver)
│   ├── Company.cs           ← New: separated from job data
│   └── User.cs              ← New: multi-tenant support
├── Enums/
│   ├── ApplicationStatus.cs ← Replaces magic strings
│   ├── SeniorityLevel.cs    ← From Silver layer string → enum
│   └── WorkModality.cs      ← New: Remote/Hybrid/OnSite
├── ValueObjects/
│   ├── TechStack.cs         ← Gold layer categories
│   └── DateRange.cs         ← Reusable date range
└── Interfaces/
    ├── IJobApplicationRepository.cs
    ├── ICompanyRepository.cs
    └── IUserRepository.cs
```
