# Day 25 — MVP Feature Completion

**Date:** 2026-03-13
**Phase:** Phase 4 — DevOps & Cloud
**Session focus:** Completing all pending MVP features — edit, search, stale notifications, appliedAt fix, and integration test expansion

---

## What I did today

### 1. Fix: appliedAt auto-set on status change

**Problem:** The `JobApplication` entity had two `UpdateStatus` overloads:

```csharp
// Overload 1 — auto-set AppliedAt ✅
public void UpdateStatus(ApplicationStatus newStatus) { ... }

// Overload 2 — accepts notes, but did NOT auto-set AppliedAt ❌
public void UpdateStatus(ApplicationStatus newStatus, string? notes) { ... }
```

The handler was calling overload 2, so transitioning to `Applied` never recorded `AppliedAt`.

**Fix in `src/JobTrackerPro.Domain/Entities/JobApplication.cs`:**

Merged both into a single method with an optional `notes` parameter:

```csharp
public void UpdateStatus(ApplicationStatus newStatus, string? notes = null)
{
    Status = newStatus;
    if (notes is not null)
        Notes = notes;
    UpdatedAt = DateTime.UtcNow;

    if (newStatus == ApplicationStatus.Applied && !AppliedAt.HasValue)
        AppliedAt = DateTime.UtcNow;
}
```

---

### 2. Add updatedAt to JobApplicationDto

Added `DateTime? UpdatedAt` to `JobApplicationDto` and to the query handler mapping so the frontend can use it to detect stale applications.

**Files changed:**
- `src/JobTrackerPro.Application/DTOs/JobApplicationDto.cs`
- `src/JobTrackerPro.Application/JobApplications/Queries/GetJobApplicationsHandler.cs`

---

### 3. New endpoint: Edit job application

Added a `PATCH /api/jobapplications/{id}` endpoint to allow editing the title, job URL, and notes of an existing application. Status changes remain on the existing `PUT` endpoint.

**New files:**
- `src/JobTrackerPro.Application/JobApplications/Commands/EditJobApplicationCommand.cs`
- `src/JobTrackerPro.Application/JobApplications/Commands/EditJobApplicationHandler.cs`

**Domain method added to `JobApplication`:**

```csharp
public void UpdateDetails(string title, string? jobUrl, string? notes)
{
    if (!string.IsNullOrWhiteSpace(title))
        Title = title.Trim();
    JobUrl = jobUrl?.Trim();
    if (notes is not null)
        Notes = notes;
    UpdatedAt = DateTime.UtcNow;
}
```

**Controller addition in `JobApplicationsController.cs`:**

```csharp
[HttpPatch("{id:guid}")]
public async Task<IActionResult> Edit(Guid id, [FromBody] EditJobApplicationCommand command, ...)
{
    var result = await _sender.Send(command with { Id = id }, cancellationToken);
    return result ? NoContent() : NotFound();
}
```

---

### 4. Frontend — Search bar

Added a text input next to the status filter in `DashboardPage.tsx`. Filters client-side by job title or company name, case-insensitive:

```tsx
const filtered = applications
  .filter(a => filter === 'all' || a.status === filter)
  .filter(a => {
    if (!search.trim()) return true;
    const q = search.toLowerCase();
    return a.title.toLowerCase().includes(q) || a.companyName.toLowerCase().includes(q);
  });
```

---

### 5. Frontend — Edit application modal

New component: `frontend/src/components/EditApplicationModal.tsx`

- Pre-fills title, job URL, and notes from the current application
- Calls `PATCH /api/jobapplications/{id}` on submit
- Updates local state optimistically after save
- Added "Edit" button to `JobApplicationCard` alongside the existing "Delete"

Also updated the card to show `companyName` and a 2-line preview of `notes`.

---

### 6. Frontend — Stale applications warning banner

Applications in active statuses (Applied, PhoneScreen, Interview, TechnicalTest, FinalInterview) that have had no activity for 7+ days trigger an amber warning banner at the top of the dashboard:

```tsx
const staleApps = applications.filter(a => {
  if (!ACTIVE_STATUSES.includes(a.status)) return false;
  const lastActivity = new Date(a.updatedAt ?? a.createdAt);
  const daysSince = (Date.now() - lastActivity.getTime()) / (1000 * 60 * 60 * 24);
  return daysSince >= STALE_DAYS; // STALE_DAYS = 7
});
```

The banner lists each stale application by title, company, and current status.

---

### 7. Integration test expansion

Expanded `JobApplicationsControllerTests.cs` from **4 tests to 12**:

| Test | What it verifies |
|------|-----------------|
| `GetJobApplications_WithoutToken_ShouldReturn401` | Auth guard on GET |
| `GetJobApplications_AfterCreating_ShouldReturnApplications` | Full create + list flow |
| `GetJobApplications_ShouldReturnStatusAsInt` | Status is returned as `int`, not string |
| `CreateJobApplication_WithValidToken_ShouldReturn201` | Happy path create |
| `CreateJobApplication_WithInvalidData_ShouldReturn400` | Validation guard |
| `CreateJobApplication_WithoutToken_ShouldReturn401` | Auth guard on POST |
| `UpdateStatus_WithValidId_ShouldReturn204` | Status update happy path |
| `UpdateStatus_WithUnknownId_ShouldReturn404` | Not found on PUT |
| `Edit_WithValidId_ShouldReturn204AndPersistChanges` | Edit endpoint + persistence |
| `Edit_WithUnknownId_ShouldReturn404` | Not found on PATCH |
| `Delete_WithValidId_ShouldReturn204AndRemoveApplication` | Delete + list empty |
| `Delete_WithUnknownId_ShouldReturn404` | Not found on DELETE |

---

## Commits made today

```
feat: complete all pending features for MVP
docs: add CLAUDE.md with project architecture and decisions
```

---

## Lessons learned

- **Overload resolution matters in domain logic** — having two methods with similar signatures where only one implements the full behavior causes silent bugs. Always prefer a single method with optional parameters over multiple overloads when the behavior is the same.
- **Client-side search is good enough for MVP** — no need for a backend search endpoint when the dataset is small. Add server-side search/pagination only when the list actually becomes large.
- **Stale detection doesn't require a notification service** — a simple computed value on the existing data (`updatedAt` vs today) is sufficient for in-app awareness without adding complexity like emails or push notifications.
- **PATCH vs PUT semantics** — use `PUT` for full resource replacement (status update replaces the status), `PATCH` for partial edits (edit only touches title/URL/notes).

---

## Current project status

### Production URLs

| Resource | URL |
|----------|-----|
| API (Swagger) | https://jobtracker-api-prod-ehg6euckd4evaabw.centralus-01.azurewebsites.net |
| Frontend | https://gleaming-lollipop-3b4183.netlify.app |
| Database | Neon.tech — ep-crimson-thunder (Azure East US) |

### Feature status

| Feature | Status |
|---------|--------|
| Auth (register/login/refresh) | Done |
| Add job application | Done |
| View dashboard with stats | Done |
| Filter by status | Done |
| Search by title/company | Done |
| Edit application (title, URL, notes) | Done |
| Update status | Done |
| Delete application | Done |
| appliedAt auto-set | Done |
| Stale application warning | Done |
| CI/CD (Azure + Netlify) | Done |
| Integration tests (12 cases) | Done |
| Redis in production | Not configured — graceful fallback active |

### CI/CD status

```
✅ Build and Test     — 12 integration + 10 unit = 22 tests
✅ Docker Build       — jobtracker-api:latest
✅ Deploy API         — Azure App Service (Central US, Free F1)
✅ Deploy Frontend    — Netlify (gleaming-lollipop-3b4183)
```
