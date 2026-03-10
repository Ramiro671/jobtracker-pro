# Day 12 — Serilog: Structured Logging

**Date:** March 10, 2026
**Phase:** 1 — Backend Core
**Block:** Bloque 14 — Serilog: Structured Logging
**Duration:** ~45 min

---

## What I did

Replaced the default ASP.NET Core logging with Serilog.
Added structured logging to handlers and configured request logging middleware.

---

## Files created / modified

### Api (modified)
- `Program.cs` — replaced default ILogger with Serilog bootstrap + UseSerilogRequestLogging
- `appsettings.json` — added Serilog MinimumLevel configuration

### Application (modified)
- `Auth/Commands/RegisterHandler.cs` — added ILogger + structured log events
- `JobApplications/Commands/CreateJobApplicationHandler.cs` — added ILogger + structured log events

### Root
- `.gitignore` — added `logs/` and `*.log` entries

---

## NuGet packages added

```bash
dotnet add src/JobTrackerPro.Api package Serilog.AspNetCore
dotnet add src/JobTrackerPro.Api package Serilog.Sinks.Console
dotnet add src/JobTrackerPro.Api package Serilog.Sinks.File
```

---

## Configuration

### Program.cs — Bootstrap logger

```csharp
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting JobTracker Pro API");
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) =>
        configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
            .WriteTo.File(
                path: "logs/jobtracker-.log",
                rollingInterval: RollingInterval.Day));

    // ...

    app.UseSerilogRequestLogging(); // logs every HTTP request automatically
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
```

### appsettings.json — Log levels

```json
"Serilog": {
  "MinimumLevel": {
    "Default": "Information",
    "Override": {
      "Microsoft": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning",
      "System": "Warning"
    }
  }
}
```

The `Override` section silences EF Core SQL query spam and Microsoft framework noise,
keeping only application-level logs visible.

---

## Structured logging in handlers

### Why structured logging?

```csharp
// ❌ Plain text — unsearchable
_logger.LogInformation("User registered with email ramiro@test.com");

// ✅ Structured — searchable, filterable, queryable
_logger.LogInformation("User {UserId} registered successfully", user.Id);
// → stored as: { Message: "User ... registered", UserId: "b115508f-..." }
```

With structured logging, production tools (Seq, Application Insights, Datadog)
can query: `WHERE UserId = 'b115508f-...'` instead of searching raw text.

### RegisterHandler events

```csharp
_logger.LogInformation("Registering new user with email {Email}", request.Email);
_logger.LogWarning("Registration failed — email {Email} already exists", request.Email);
_logger.LogInformation("User {UserId} registered successfully", user.Id);
```

### CreateJobApplicationHandler events

```csharp
_logger.LogInformation(
    "Creating job application for user {UserId} at company {Company}",
    command.UserId, command.CompanyName);

_logger.LogInformation(
    "Job application {ApplicationId} created successfully", application.Id);
```

---

## Log levels used

| Level | When to use |
|-------|-------------|
| `LogTrace` | Very detailed — loop iterations, raw data |
| `LogDebug` | Diagnostic info for debugging |
| `LogInformation` | Normal flow — user registered, request processed |
| `LogWarning` | Unexpected but handled — email already exists, 404 |
| `LogError` | Unhandled exceptions — 500 errors |
| `LogFatal` | App crash — startup failure |

---

## Output observed

```
[17:20:01 INF] Starting JobTracker Pro API
[17:27:16 INF] HTTP GET /index.html responded 304 in 383.1459 ms
               {"SourceContext": "Serilog.AspNetCore.RequestLoggingMiddleware",
                "RequestId": "0HNJUVI8DOSR0:00000001",
                "ConnectionId": "0HNJUVI8DOSR0"}
[17:27:20 INF] HTTP GET /swagger/v1/swagger.json responded 200 in 651.1721 ms
```

Every HTTP request is automatically logged with:
- HTTP method + path
- Status code
- Response time in ms
- RequestId (for distributed tracing)
- ConnectionId

---

## Rolling file logs

```
logs/
  jobtracker-20260310.log   ← today
  jobtracker-20260311.log   ← tomorrow (auto-created)
  jobtracker-20260312.log   ← etc.
```

Old logs survive API restarts. New file created at midnight automatically.
Added `logs/` to `.gitignore` — log files must never be committed.

---

## Phase 1 progress

| Block | Content | Status |
|-------|---------|--------|
| 1 | Clean Architecture setup | ✅ |
| 2-3 | Domain: Entities + Interfaces | ✅ |
| 4 | Application: MediatR + DTOs | ✅ |
| 5 | Infrastructure: Repositories + DI | ✅ |
| 6 | API: Controller + Swagger | ✅ |
| 7 | Docker + PostgreSQL + Migrations | ✅ |
| 8 | Repository Pattern: Full implementation | ✅ |
| 9 | CRUD Completo: PUT + DELETE | ✅ |
| 10 | FluentValidation + Global Error Handler | ✅ |
| 11 | JWT Auth: Register + Login | ✅ |
| 12 | Refresh Token + [Authorize] + Swagger Bearer | ✅ |
| 13 | Clean Architecture Audit + Refactor | ✅ |
| **14** | **Serilog: Structured Logging** | ✅ |
| 15 | Semana 3 Review + README Avanzado | ⏳ next |

---

## Commit

```
feat: add structured logging with Serilog
```
