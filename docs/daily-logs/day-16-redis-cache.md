# Day 16 — Redis: Cache Layer

**Date:** March 11, 2026
**Phase:** 2 — DevOps
**Block:** Bloque 19 — Redis: Cache Layer
**Duration:** ~30 min

---

## What I did

Added a Redis-backed distributed cache layer using the ICacheService abstraction.
GET job applications now served from cache — DB only queried on cache miss.
POST/PUT/DELETE invalidate the cache for the affected user.

---

## Files created / modified

| File | Action |
|------|--------|
| `Application/Common/Interfaces/ICacheService.cs` | Created — interface |
| `Infrastructure/Caching/RedisCacheService.cs` | Created — implementation |
| `Application/JobApplications/Queries/GetJobApplicationsHandler.cs` | Updated — cache-aside pattern |
| `Application/JobApplications/Commands/CreateJobApplicationHandler.cs` | Updated — cache invalidation |
| `Infrastructure/DependencyInjection.cs` | Updated — Redis + ICacheService registered |
| `appsettings.json` | Updated — added Redis connection string |

---

## NuGet packages added

```bash
dotnet add src/JobTrackerPro.Infrastructure package StackExchange.Redis
dotnet add src/JobTrackerPro.Infrastructure package Microsoft.Extensions.Caching.StackExchangeRedis
```

---

## Cache-aside pattern

```
GET /api/jobapplications/{userId}
    ↓
GetJobApplicationsHandler
    ↓ cache.GetAsync("job-applications:{userId}")
    ├── HIT  → return cached DTOs immediately (no DB call)
    └── MISS → query PostgreSQL → store in Redis (10 min TTL) → return
```

```
POST /api/jobapplications (create new)
    ↓
CreateJobApplicationHandler
    ↓ save to PostgreSQL
    ↓ cache.RemoveAsync("job-applications:{userId}")  ← invalidate
```

---

## ICacheService interface (Application layer)

```csharp
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, ...);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, ...);
    Task RemoveAsync(string key, ...);
}
```

Interface lives in **Application** — Infrastructure provides the Redis implementation.
Follows the same Dependency Inversion pattern as IJwtTokenGenerator.

---

## Redis DI registration

```csharp
services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnection; // "localhost:6379"
    options.InstanceName = "JobTrackerPro:";
});

services.AddScoped<ICacheService, RedisCacheService>();
```

---

## Fix — Unit tests broke

`GetJobApplicationsHandler` and `CreateJobApplicationHandler` now require
`ICacheService` in their constructors.

Added `Mock<ICacheService>` to both unit test constructors:

```csharp
private readonly Mock<ICacheService> _cacheMock = new();

_handler = new GetJobApplicationsHandler(
    _repositoryMock.Object,
    _cacheMock.Object);  // ← added
```

All 11 tests still passing after fix. ✅

---

## Cache key strategy

```
"job-applications:{userId}"   e.g. "job-applications:b115508f-7d93-40de-93fa-..."
```

TTL: 10 minutes (configurable).
Redis instance prefix: `JobTrackerPro:` (avoids key collisions in shared Redis).

---

## Commit

```
feat: add Redis cache layer with cache-aside pattern for job applications
```
