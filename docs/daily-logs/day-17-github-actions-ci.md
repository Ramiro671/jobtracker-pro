# Day 17 — GitHub Actions: CI Pipeline

**Date:** March 11, 2026
**Phase:** 2 — DevOps
**Block:** Bloque 20 — GitHub Actions CI Pipeline
**Duration:** ~20 min

---

## What I did

Created a GitHub Actions CI pipeline with two jobs:
build-and-test (runs all tests) and docker-build (verifies the image builds).
Pipeline triggers on push to main/develop and on pull requests to main.

---

## Files created

```
.github/
└── workflows/
    └── ci.yml
```

---

## Pipeline structure

```
Trigger: push → main/develop  |  PR → main
                │
        ┌───────▼────────┐
        │ build-and-test │  ubuntu-latest
        │                │  postgres:16-alpine service
        │  dotnet restore│
        │  dotnet build  │
        │  unit tests    │
        │  integration   │
        └───────┬────────┘
                │ needs: build-and-test
        ┌───────▼────────┐
        │  docker-build  │  ubuntu-latest
        │                │
        │  docker build  │
        │  docker images │
        └────────────────┘
```

---

## ci.yml — key sections

### PostgreSQL service for integration tests

```yaml
services:
  postgres:
    image: postgres:16-alpine
    env:
      POSTGRES_DB: jobtracker_test
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgres
    ports:
      - 5432:5432
    options: >-
      --health-cmd pg_isready
      --health-interval 5s
      --health-retries 5
```

The integration tests use InMemory DB — the postgres service is available
for future tests that require a real DB connection.

### Two separate test runs

```yaml
- name: Run unit tests
  run: dotnet test tests/JobTrackerPro.UnitTests ...

- name: Run integration tests
  run: dotnet test tests/JobTrackerPro.IntegrationTests ...
```

Separated so failures are reported per test project.

### Docker build job

```yaml
docker-build:
  needs: build-and-test   ← only runs if tests pass
  steps:
    - docker build -t jobtracker-api:${{ github.sha }} .
    - docker images jobtracker-api
```

Tags the image with the commit SHA — every build is traceable.

---

## What CI catches automatically

| Scenario | CI response |
|----------|-------------|
| Code that breaks a test | ❌ Pipeline fails → PR blocked |
| Code that breaks the build | ❌ Pipeline fails → PR blocked |
| Dockerfile broken | ❌ docker-build job fails |
| All green | ✅ PR can be merged |

---

## Phase 2 progress

| Block | Content | Status |
|-------|---------|--------|
| 18 | Docker: Dockerfile Multi-stage | ✅ |
| 19 | Redis: Cache Layer | ✅ |
| **20** | **GitHub Actions: CI Pipeline** | ✅ |
| 21 | React + TypeScript: Setup Vite | ⏳ next |

---

## Commits

```
feat: add multi-stage Dockerfile and update docker-compose with Redis and API
feat: add Redis cache layer with cache-aside pattern for job applications
ci: add GitHub Actions CI pipeline with build, test and docker jobs
```
