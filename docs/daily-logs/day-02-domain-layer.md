# Day 02 — Domain Layer: Entities, Enums, Value Objects & Interfaces

**Date:** 2026-03-08  
**Block:** JobTracker Pro — Phase 1, Block 2  
**Duration:** ~1 hour

## What I did

- Analyzed LinkedInAgent.Grpc source code to extract reusable domain concepts
- Parsed MongoDB BSON exports (1,319 Bronze + 1,319 Silver + 3 Gold documents)
- Designed and created the Domain layer with Clean Architecture principles

## Files created

### Entities (3)
- `JobApplication.cs` — Aggregate root with behavior methods (UpdateStatus, SetTechStack, SetSalary)
- `Company.cs` — Company entity with factory method and navigation properties
- `User.cs` — User entity for JWT authentication support

### Enums (3)
- `ApplicationStatus.cs` — 9-stage pipeline replacing magic strings ("Crudo", "Procesado_Plata")
- `SeniorityLevel.cs` — 8 levels from Intern to Principal (from Silver layer data)
- `WorkModality.cs` — Remote, Hybrid, OnSite

### Value Objects (2)
- `TechStack.cs` — 5 categories aligned with Gold layer analysis (Backend, Frontend, Databases, CloudAndDevOps, Testing)
- `DateRange.cs` — Reusable date range with DaysElapsed calculation

### Interfaces (3)
- `IJobApplicationRepository.cs` — CRUD + filter by status
- `ICompanyRepository.cs` — CRUD + GetByName to avoid duplicates
- `IUserRepository.cs` — CRUD + GetByEmail for login + Exists for registration

## Key improvements over LinkedInAgent.Grpc
- No hardcoded secrets (connection strings will use appsettings + env vars)
- All interfaces defined (no concrete dependencies in Domain)
- No god classes (separated repositories per entity)
- English everywhere (variables, methods, XML comments)
- Type-safe enums instead of magic strings
- Private setters + factory methods for controlled entity creation

## Build result
All 6 projects compiled successfully (0 errors, 0 warnings).

## Next step
Block 3 — EF Core DbContext + PostgreSQL configuration in Infrastructure layer.
