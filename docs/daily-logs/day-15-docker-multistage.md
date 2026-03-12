# Day 15 — Docker: Dockerfile Multi-stage + Compose

**Date:** March 11, 2026
**Phase:** 2 — DevOps
**Block:** Bloque 18 — Docker: Dockerfile Multi-stage + Compose
**Duration:** ~30 min

---

## What I did

Created a production-grade multi-stage Dockerfile and updated docker-compose.yml
to include PostgreSQL + Redis + API as a full local stack.

---

## Files created / modified

| File | Action |
|------|--------|
| `Dockerfile` | Created — 2-stage build |
| `.dockerignore` | Created — excludes tests, docs, obj, bin |
| `docker-compose.yml` | Updated — added Redis + API services |

---

## Dockerfile — 2 stages

```
Stage 1: build  (mcr.microsoft.com/dotnet/sdk:10.0)
  → dotnet restore
  → dotnet publish -c Release → /app/publish

Stage 2: runtime  (mcr.microsoft.com/dotnet/aspnet:10.0)
  → COPY --from=build /app/publish
  → non-root user (appuser)
  → EXPOSE 8080
```

**Why multi-stage?**
- Build image: ~750MB (SDK included)
- Runtime image: ~106MB (only ASP.NET runtime)
- Production ships the runtime image only — 7x smaller

---

## Fixes applied

### .slnx format
.NET 10 uses `.slnx` instead of `.sln`.
Dockerfile updated to reference `JobTrackerPro.slnx`.

### Debian base image — user creation
`mcr.microsoft.com/dotnet/aspnet:10.0` is Debian, not Alpine.
```bash
# ❌ Alpine syntax
RUN addgroup --system appgroup && adduser --system --ingroup appgroup appuser

# ✅ Debian syntax
RUN groupadd --system appgroup && useradd --system --gid appgroup appuser
```

---

## docker-compose.yml — full stack

```yaml
services:
  postgres:   ← PostgreSQL 16-alpine + healthcheck
  redis:      ← Redis 7-alpine + healthcheck
  api:        ← Built from Dockerfile, depends_on both
```

API waits for both postgres and redis to be healthy before starting.

---

## Result

```
docker build -t jobtracker-api .   ✅
Image size: 106 MB (377 MB on disk with layers)
Image ID: d84401b164ab
```

---

## Commit

```
feat: add multi-stage Dockerfile and update docker-compose with Redis and API
```
