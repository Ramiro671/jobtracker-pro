# Day 18 — React + TypeScript + Vite Setup (Bloque 21)
# Day 19 — Auth Pages + JWT Context (Bloque 22)
# Day 20 — Dashboard + CRUD UI (Bloque 23)
# Day 21 — Frontend Integration: Router + ProtectedRoute (Bloque 24)

**Date:** March 11, 2026
**Phase:** 3 — React Frontend
**Blocks:** 21 · 22 · 23 · 24
**Duration:** ~2 hours

---

## What I did

Built the complete React + TypeScript frontend from scratch.
Register → Login → Dashboard → Add/Update/Delete job applications.
Full integration with the ASP.NET Core API via Axios + JWT.

---

## Result

```
npm run dev → http://localhost:5174 ✅
Dashboard loading with stats: Total · Active · Offers ✅
+ Add Application button and modal ✅
Status filter dropdown ✅
No critical console errors ✅
```

---

## Files created

```
frontend/
├── src/
│   ├── api/
│   │   ├── client.ts              — Axios instance + JWT interceptor + 401 handler
│   │   └── jobApplications.ts     — API calls: getAll, create, updateStatus, delete
│   ├── context/
│   │   └── AuthContext.tsx        — AuthProvider + useAuth hook + JWT decode
│   ├── components/
│   │   ├── JobApplicationCard.tsx — Card with status badge + select + delete
│   │   ├── AddApplicationModal.tsx — Modal form: title, company, URL, source
│   │   └── ProtectedRoute.tsx     — Redirects unauthenticated users to /login
│   ├── pages/
│   │   ├── LoginPage.tsx          — Email + password form
│   │   ├── RegisterPage.tsx       — Full name + email + password form
│   │   └── DashboardPage.tsx      — Stats + filter + cards grid
│   ├── types/
│   │   └── index.ts               — ApplicationStatus enum + STATUS_LABELS + STATUS_COLORS
│   ├── App.tsx                    — BrowserRouter + Routes
│   ├── main.tsx                   — ReactDOM.createRoot
│   └── index.css                  — Tailwind directives
├── .env                           — VITE_API_URL=http://localhost:5086
├── tailwind.config.js
└── vite.config.ts
```

---

## NuGet / npm packages installed

```bash
npm create vite@latest frontend -- --template react-ts
npm install axios react-router-dom @tanstack/react-query
npm install -D tailwindcss postcss autoprefixer
npx tailwindcss init -p
```

---

## BLOQUE 21 — React + TypeScript + Vite

### Key files

**`api/client.ts`** — Axios base instance:
```typescript
// Attach JWT to every request
apiClient.interceptors.request.use((config) => {
  const token = localStorage.getItem('accessToken');
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

// Auto-redirect to /login on 401
apiClient.interceptors.response.use(
  (res) => res,
  (error) => {
    if (error.response?.status === 401) {
      localStorage.clear();
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);
```

**`types/index.ts`** — ApplicationStatus enum mirrors the backend C# enum:
```typescript
export enum ApplicationStatus {
  Applied = 0, PhoneScreen = 1, Interview = 2,
  TechnicalTest = 3, FinalInterview = 4,
  OfferReceived = 5, OfferAccepted = 6,
  Rejected = 7, Withdrawn = 8,
}
```

### Fix — Node.js version warning
Vite 7 requires Node.js 20.19+ or 22.12+.
Running Node 20.16 triggers a warning but Vite still works.
Solution: downgrade to `vite@5` or upgrade Node to 22 LTS.

---

## BLOQUE 22 — Auth Pages + JWT Context

### AuthContext.tsx — JWT decode without library

```typescript
// Decode userId from JWT payload (no jwt-decode library needed)
const payload = JSON.parse(atob(res.data.accessToken.split('.')[1]));
saveAuth(res.data, payload.sub); // sub = User ID (Guid)
```

### Auth flow
```
Register → POST /api/auth/register → save tokens → navigate /dashboard
Login    → POST /api/auth/login    → save tokens → navigate /dashboard
Logout   → localStorage.clear()   → navigate /login
401 response → auto-logout via Axios interceptor
```

### Pages
- `LoginPage.tsx` — email + password + error state + loading state
- `RegisterPage.tsx` — fullName + email + password + error + loading

Both use `useAuth()` hook and `useNavigate()` from react-router-dom.

---

## BLOQUE 23 — Dashboard + CRUD UI

### DashboardPage.tsx — stats calculation

```typescript
const stats = {
  total: applications.length,
  active: applications.filter(a =>
    a.status < ApplicationStatus.OfferAccepted &&
    a.status !== ApplicationStatus.Rejected &&
    a.status !== ApplicationStatus.Withdrawn
  ).length,
  offers: applications.filter(a =>
    a.status === ApplicationStatus.OfferReceived ||
    a.status === ApplicationStatus.OfferAccepted
  ).length,
};
```

### JobApplicationCard.tsx
- Status badge with color coding (STATUS_COLORS map)
- Inline `<select>` to change status → PUT /api/jobapplications/{id}
- Delete button → DELETE /api/jobapplications/{id}
- Optimistic UI: status updates reflected immediately without refetch

### AddApplicationModal.tsx
- Fields: title (required), company (required), jobUrl, description, source
- Source dropdown: LinkedIn · Indeed · Direct · Referral · Other
- On submit → POST /api/jobapplications → refetch list

---

## BLOQUE 24 — Router + ProtectedRoute + Wiring

### App.tsx — route structure

```typescript
<Routes>
  <Route path="/login"     element={<LoginPage />} />
  <Route path="/register"  element={<RegisterPage />} />
  <Route path="/dashboard" element={
    <ProtectedRoute>
      <DashboardPage />
    </ProtectedRoute>
  } />
  <Route path="*" element={<Navigate to="/login" replace />} />
</Routes>
```

### ProtectedRoute.tsx

```typescript
export default function ProtectedRoute({ children }) {
  const { isAuthenticated } = useAuth();
  return isAuthenticated ? <>{children}</> : <Navigate to="/login" replace />;
}
```

Unauthenticated users → redirected to /login automatically.
After login → redirected to /dashboard automatically.

---

## Issues fixed

| Issue | Fix |
|-------|-----|
| Redis not running → 500 on GET | `docker-compose up postgres redis -d` |
| Node 20.16 warning | Vite still works, upgrade Node to 22 LTS pending |
| CORS — OPTIONS 405 | Added CORS policy in Program.cs |
| CORS — OPTIONS 204 | CORS working after fix ✅ |

---

## Phase 3 progress

| Block | Content | Status |
|-------|---------|--------|
| **21** | React + TypeScript + Vite + Tailwind | ✅ |
| **22** | AuthContext + Login + Register pages | ✅ |
| **23** | Dashboard + JobApplicationCard + Modal | ✅ |
| **24** | Router + ProtectedRoute + wiring final | ✅ |

---

## Overall project progress

| Phase | Blocks | Status |
|-------|--------|--------|
| 1 — Backend Core | 1–17 | ✅ COMPLETE |
| 2 — DevOps | 18–20 | ✅ COMPLETE |
| 3 — React Frontend | 21–24 | ✅ COMPLETE |
| 4 — Deploy | 25–28 | ⏳ next |
| 5 — Portfolio | 29–33 | ⏳ |

---

## Commit

```
feat: add React TypeScript frontend with auth, dashboard and CRUD UI
```
