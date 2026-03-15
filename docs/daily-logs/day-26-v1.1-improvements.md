# Day 26 — v1.1.0 Improvements

**Date:** 2026-03-14
**Phase:** Post-MVP Enhancements
**Session focus:** Dark mode, CSV export, change password, automatic token refresh, stale email notifications, interactive study guide

---

## What was done in this session

After completing the MVP (v1.0.0) the following improvements were added in a single commit:

---

### 1. Dark Mode (Tailwind CSS v4)

**Why Tailwind v4 is different:**
Tailwind v4 uses `@tailwindcss/vite` — there is no `tailwind.config.js`. Dark mode must be declared as a CSS variant:

```css
/* frontend/src/index.css */
@import "tailwindcss";
@variant dark (&:where(.dark, .dark *));
```

**ThemeContext.tsx** (new):
- Stores `theme: 'light' | 'dark'` in `useState`
- On change: adds/removes `.dark` class on `document.documentElement`
- Persists selection to `localStorage`
- Exported via `useTheme()` hook

**Files changed:**
- `frontend/src/context/ThemeContext.tsx` — new context
- `frontend/src/App.tsx` — wrapped app with `<ThemeProvider>`
- `frontend/src/pages/DashboardPage.tsx` — toggle button (sun/moon icon)
- `frontend/src/components/JobApplicationCard.tsx` — all `dark:` variants
- `frontend/src/components/AddApplicationModal.tsx` — all `dark:` variants
- `frontend/src/components/EditApplicationModal.tsx` — all `dark:` variants

---

### 2. CSV Export

Client-side only — no backend endpoint required.

```typescript
const exportCSV = () => {
  const rows = applications.map(a => [
    a.title, a.companyName, STATUS_LABELS[a.status],
    a.jobUrl ?? '', a.source ?? '', a.notes ?? '',
    new Date(a.createdAt).toLocaleDateString(),
  ]);
  const csv = [
    'Title,Company,Status,URL,Source,Notes,Created',
    ...rows.map(r => r.map(v => `"${String(v).replace(/"/g, '""')}"`).join(','))
  ].join('\n');
  const blob = new Blob([csv], { type: 'text/csv' });
  const url = URL.createObjectURL(blob);
  const a = document.createElement('a');
  a.href = url;
  a.download = `jobtracker-export-${new Date().toISOString().slice(0,10)}.csv`;
  a.click();
  URL.revokeObjectURL(url);
};
```

**Why `URL.revokeObjectURL` matters:** releases the in-memory blob URL after download, preventing memory leaks.

---

### 3. Change Password — Full Stack

**Backend:**

New files:
- `src/JobTrackerPro.Application/Users/Commands/ChangePasswordCommand.cs` — record with UserId, CurrentPassword, NewPassword; implements `IRequest` (no return value)
- `src/JobTrackerPro.Application/Users/Commands/ChangePasswordHandler.cs` — validates current password with BCrypt, hashes new password, calls `user.ChangePassword(newHash)`, saves
- `src/JobTrackerPro.Api/Controllers/UsersController.cs` — `PUT /api/users/me/password`; extracts userId from JWT `sub` claim

Domain method added to `User.cs`:
```csharp
public void ChangePassword(string newPasswordHash)
{
    if (string.IsNullOrWhiteSpace(newPasswordHash))
        throw new ArgumentException("Password hash is required.", nameof(newPasswordHash));
    PasswordHash = newPasswordHash;
}
```

**Key detail — JWT claim extraction:**
```csharp
var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub");
```
`ClaimTypes.NameIdentifier` maps to the `sub` claim in ASP.NET Core JWT middleware.

**Frontend:**

New component: `frontend/src/components/ChangePasswordModal.tsx`
- Three fields: Current Password, New Password, Confirm New Password
- Client-side validation: passwords match, min 8 chars
- On success: calls `showSuccess()` and closes modal
- On error: calls `showError()` with API message

---

### 4. Automatic JWT Token Refresh

**Problem:** When the access token expires (60 min), all concurrent API calls return 401 simultaneously. Without queuing, each call tries to refresh independently → race condition → multiple refresh requests → token rotation issues.

**Solution — Queue pattern in `frontend/src/api/client.ts`:**

```typescript
let isRefreshing = false;
let pendingQueue: Array<{
  resolve: (token: string) => void;
  reject: (err: unknown) => void;
}> = [];

const drainQueue = (err: unknown, token: string | null) => {
  pendingQueue.forEach(p => err ? p.reject(err) : p.resolve(token!));
  pendingQueue = [];
};

apiClient.interceptors.response.use(null, async (error) => {
  const config = error.config;
  if (error.response?.status !== 401 || config._retry) throw error;

  if (isRefreshing) {
    return new Promise((resolve, reject) =>
      pendingQueue.push({ resolve, reject })
    ).then(token => {
      config.headers.Authorization = `Bearer ${token}`;
      return apiClient(config);
    });
  }

  config._retry = true;
  isRefreshing = true;

  try {
    const { accessToken } = await refreshToken();
    localStorage.setItem('accessToken', accessToken);
    apiClient.defaults.headers.common.Authorization = `Bearer ${accessToken}`;
    drainQueue(null, accessToken);
    config.headers.Authorization = `Bearer ${accessToken}`;
    return apiClient(config);
  } catch (refreshError) {
    drainQueue(refreshError, null);
    localStorage.removeItem('accessToken');
    localStorage.removeItem('userId');
    window.location.href = '/login';
    throw refreshError;
  } finally {
    isRefreshing = false;
  }
});
```

**Flow:**
1. Request fails with 401 → interceptor fires
2. If already refreshing: add to `pendingQueue` (waits for Promise resolution)
3. If not refreshing: set `isRefreshing = true`, call `POST /api/auth/refresh`
4. On success: update token, `drainQueue(null, newToken)` → all queued requests retry
5. On failure: `drainQueue(error, null)` → all queued requests reject → redirect to `/login`

---

### 5. Stale Email Notifications

**Backend services added:**

`src/JobTrackerPro.Application/Common/Interfaces/IEmailService.cs`:
```csharp
public interface IEmailService
{
    Task SendAsync(string to, string subject, string htmlBody,
        CancellationToken cancellationToken = default);
}
```

`src/JobTrackerPro.Infrastructure/Email/SmtpEmailService.cs`:
- When `EmailSettings:Enabled = false`: logs at Information level and returns immediately (no exception)
- When enabled: sends via `System.Net.Mail.SmtpClient` on port 587 with SSL

`src/JobTrackerPro.Infrastructure/BackgroundServices/StaleNotificationService.cs`:
- Extends `BackgroundService` → registered as singleton hosted service
- Uses `IServiceScopeFactory` to create a new scope per iteration (DbContext is scoped, BackgroundService is singleton)
- Queries applications where status is in active states AND `(UpdatedAt ?? CreatedAt) < DateTime.UtcNow - 7 days`
- Fetches user records by UserId list (avoids missing navigation property)
- Groups stale apps by user → sends 1 email per user listing their stale applications

**Why `IServiceScopeFactory`?**
BackgroundService lifetime = Singleton. DbContext lifetime = Scoped.
You cannot inject a Scoped service into a Singleton directly.
Solution: inject `IServiceScopeFactory`, create a scope per run, resolve services from the scope.

**Registration in `DependencyInjection.cs`:**
```csharp
services.AddScoped<IEmailService, SmtpEmailService>();
services.AddHostedService<StaleNotificationService>();
```

---

### 6. Interactive Study Guide + Study Guide Prompt

- `docs/daily-logs/interactive-guide.html` — standalone HTML file (no server needed)
  - 10 modules: Stack, Architecture, Full Flow, JWT Auth, Domain, Application CQRS, Infrastructure, API, Frontend, CI/CD
  - Animated request flow (13 steps from React form to PostgreSQL and back)
  - Keyboard navigation (← →, 1-0)
- `docs/daily-logs/study-guide-prompt.txt` — prompt for Claude Chat to generate a 14-section bilingual study guide with 60 interview Q&A, 40 flashcards, 5 ASCII mind maps, and a 30-question exam

---

## Commits in this session

```
feat: add dark mode, CSV export, password change, token refresh, and stale email notifications
docs: add interactive study guide and study guide prompt for interview prep
```

---

## Current project status as of v1.1.0

### Production URLs

| Resource | URL |
|----------|-----|
| API (Swagger) | https://jobtracker-api-prod-ehg6euckd4evaabw.centralus-01.azurewebsites.net |
| Frontend | https://gleaming-lollipop-3b4183.netlify.app |
| Database | Neon.tech (serverless PostgreSQL, Azure East US) |

### Demo credentials
| Field | Value |
|-------|-------|
| Email | demo@jobtracker.dev |
| Password | Demo1234! |

### Feature checklist

| Feature | Status |
|---------|--------|
| Auth (register/login/refresh/logout) | Done |
| Auto JWT refresh with request queue | Done |
| Change password | Done |
| Add / Edit / Delete job application | Done |
| Update application status | Done |
| Dashboard stats (Total / Active / Offers) | Done |
| Search by title or company | Done |
| Filter by status | Done |
| Stale warning banner (7-day threshold) | Done |
| Dark mode | Done |
| CSV export | Done |
| Pagination (12 per page) | Done |
| Stale email notifications (backend) | Done (disabled, logs only) |
| Redis caching | Done (graceful fallback in prod) |
| CI/CD (Azure + Netlify) | Done |
| Tests: 10 unit + 12 integration = 22 | Done |

### Entity fields in DB (not yet in API/Frontend)

The `JobApplications` table already has these columns (from `InitialCreate` migration),
but they are **not yet surfaced** through the API or UI:

| Field | DB Column | Type |
|-------|-----------|------|
| Full job description | `Description` | text |
| Tech stack — backend | `TechStack_Backend` | varchar(500) |
| Tech stack — frontend | `TechStack_Frontend` | varchar(500) |
| Tech stack — databases | `TechStack_Databases` | varchar(500) |
| Tech stack — cloud/DevOps | `TechStack_CloudAndDevOps` | varchar(500) |
| Tech stack — testing | `TechStack_Testing` | varchar(500) |
| Recruiter name | `ContactName` | text |
| Recruiter email | `ContactEmail` | text |

These fields are the bridge to **LinkedInAgent.Grpc** auto-fill integration (future).
