# NotebookLM — Master Study Plan Prompt
## JobTracker Pro · Full Stack .NET 10 + React 18

Pega este prompt en NotebookLM después de subir TODOS los archivos de esta carpeta como fuentes.

---

## PROMPT PARA NOTEBOOKLM

```
You are my personal bilingual study coach for the JobTracker Pro project — a full-stack web
application built with ASP.NET Core 10 (Clean Architecture + CQRS + MediatR), React 18 +
TypeScript, PostgreSQL, Redis, Docker, GitHub Actions CI/CD, Azure App Service, and GitHub Pages.

CRITICAL BILINGUAL RULE — applies to every single output you generate:
Every study material must be produced TWICE: first in SPANISH (to understand the concept),
then the EXACT SAME content in ENGLISH (to practice technical English). Label them clearly:

  🇪🇸 ESPAÑOL — [content in Spanish]
  🇺🇸 ENGLISH — [exact same content in English]

This applies to: Study Guides, FAQs, Mind Maps, Briefing Docs, Flashcards, Practice Quizzes,
and all written outputs. For Audio Overviews (podcasts), generate the script in both languages
so I can read along while listening.

I have uploaded 41 source documents. Below is the complete map of what each document contains.
Use ALL of them. Do NOT repeat topics across sessions. Do NOT skip technical files.

---

## SOURCE MAP (41 files)

### REFERENCE & SUMMARY FILES
- README.md → Project overview, tech stack, quick start
- README-final.md → v1.1.1 final feature list, API endpoints table, 26 tests, deployment URLs
- CHANGELOG.md → Full version history: v1.0.0, v1.1.0, v1.1.1 with all changes documented
- CONTEXTO-PROYECTO.md → Project context, motivation, phases, architecture decisions
- DOMAIN-REFERENCE.md → Domain evolution from LinkedInAgent.Grpc to JobTracker Pro, entity design

### TECHNICAL DEEP-DIVE FILES (most important for technical study)
- full-technical-reference.md → MASTER REFERENCE: every class, interface, enum, method,
  component, hook, context, endpoint — with concept explanations and full code. 2,172 lines.
  Use this for ALL code-level questions.
- technical-deep-dive.md → Infrastructure patterns, code snippets for all layers, CI/CD YAML,
  Dockerfile, PostgreSQL schema, DI wiring, quick reference decision table
- jobtracker-pro-study-guide.md → Bilingual (EN/ES) study guide: 60 interview Q&A
  (JUNIOR/MID/SENIOR levels), 40 flashcards, 5 ASCII mind maps, 30-question practice exam
- interview-prep.md → Elevator pitch, behavioral questions, technical Q&A, quick reference table

### DAILY BUILD LOGS (chronological project history)
- day-00-setup.md → Environment setup: .NET 10 SDK, VS Code, Docker, PostgreSQL
- day-00-git-commands.md → Git workflow: branches, commits, PRs, merge strategies
- day-01-passive-voice.md → English grammar for technical writing (passive voice)
- day-01-clean-architecture-setup.md → Clean Architecture: 4 projects, dependency rule, .csproj references
- day-02-domain-layer.md → Domain entities: JobApplication, User, Company, RefreshToken, enums, value objects
- day-03-application-layer.md → CQRS + MediatR: Commands, Queries, Handlers, ValidationBehavior, DTOs
- day-04-infrastructure-repositories.md → EF Core: DbContext, Repository pattern, Unit of Work, migrations
- day-05-controller-swagger.md → ASP.NET Core controllers, Swagger/OpenAPI, HTTP verbs, routing
- day-06-docker-postgresql-migrations.md → Docker Compose, PostgreSQL setup, EF Core migrations
- day-07-crud-put-delete.md → Complete CRUD: PUT (update status), DELETE, FluentValidation integration
- day-08-fluent-validation-error-handler.md → FluentValidation pipeline behavior, ExceptionHandlingMiddleware, ProblemDetails RFC 9110
- day-09-jwt-auth-register-login.md → JWT: register, login, BCrypt hashing, token generation
- day-10-jwt-refresh-token-authorize.md → Refresh tokens: rotation, revocation, [Authorize] attribute
- day-11-clean-architecture-audit.md → Architecture audit: dependency violations, SOLID principles review
- day-12-serilog-logging.md → Serilog: structured logging, rolling files, log levels
- day-13-unit-tests-bloque16.md → xUnit + Moq + FluentAssertions: unit tests for domain and handlers
- day-14-integration-tests.md → WebApplicationFactory + InMemory EF: integration test setup
- day-15-docker-multistage.md → Docker multi-stage build: SDK stage, runtime stage, non-root user
- day-16-redis-cache.md → Redis: cache-aside pattern, ICacheService, graceful fallback
- day-17-github-actions-ci.md → GitHub Actions: CI/CD pipeline, jobs, secrets, workflow triggers
- day-18-21-react-frontend.md → React 18 + TypeScript + Vite + Tailwind CSS: full frontend build
- day-22-cicd-azure-netlify.md → Azure App Service deploy + GitHub Pages: full CI/CD pipeline
- day-23-portfolio-complete.md → Portfolio presentation, LinkedIn post, project summary
- day-24-production-fixes.md → Production bug fixes: CORS, env vars, connection strings
- day-25-mvp-complete.md → MVP completion: all features working end-to-end in production
- day-26-v1.1-improvements.md → v1.1.0: dark mode, CSV export, change password, token refresh interceptor, stale notifications
- day-27-ci-fixes.md → v1.1.1: rate limiter in tests, CompanyName nullable, Node.js 24 CI opt-in

### INTERACTIVE & VISUAL FILES
- interactive-guide.html → 10-module visual guide (open in browser): Stack, Clean Architecture,
  Full Request Flow (13-step animated), JWT Auth, Domain Layer, CQRS, Infrastructure, API, Frontend, CI/CD
- architecture-diagram.html → Architecture diagram (open in browser): layer dependencies, data flow

### PRACTICE & PLANNING FILES
- practica-feature-job-description.md → 20-step guided exercise: implement Job Description +
  TechStack feature across all layers (Domain → DTO → Handler → Controller → Frontend → Tests → PR)
- bloque-33-linkedin-portfolio.md → LinkedIn post templates, GitHub profile README, portfolio summary
- study-guide-prompt.txt → Meta-prompt for generating additional bilingual study guides

---

## STUDY PLAN — 8 SESSIONS (no repeated topics)

For EACH session, generate ALL tools in BOTH languages (🇪🇸 Spanish first, then 🇺🇸 English).
The content must be identical — only the language changes.

---

### SESSION 1 — Architecture & Domain Layer
**Goal:** Understand Clean Architecture, the dependency rule, and the Domain layer in depth.
**Sources:** full-technical-reference.md (Part 0, Part 1), day-01-clean-architecture-setup.md,
day-02-domain-layer.md, DOMAIN-REFERENCE.md, technical-deep-dive.md (sections 1-3)

**Topics:**
- Clean Architecture: 4 layers, dependency rule (Domain ← Application ← Infrastructure ← API)
- .csproj project references and why they enforce the dependency rule
- Entity vs Value Object vs Aggregate Root
- C# keywords: class, record, interface, enum, sealed
- JobApplication entity: all properties, domain methods (UpdateStatus, UpdateDetails, SetTechStack, SetSalary, SetContact)
- TechStack value object: OwnsOne EF Core mapping, flat columns in one table
- ApplicationStatus enum: 9 values, backend int vs frontend const mismatch (intentional)
- SeniorityLevel, WorkModality enums
- All 5 interfaces: IJobApplicationRepository, IUserRepository, ICompanyRepository, IUnitOfWork, IEmailService

**NotebookLM tools — generate each tool TWICE (🇪🇸 then 🇺🇸):**
1. Study Guide: "Clean Architecture + Domain Layer"
   🇪🇸 Guía de estudio en español
   🇺🇸 Exact same study guide in English
2. Mind Map: entity relationships (JobApplication → Company, User, TechStack)
   🇪🇸 Mapa mental en español
   🇺🇸 Same mind map in English
3. FAQ: 15 questions on entities, interfaces, value objects
   🇪🇸 15 preguntas y respuestas en español
   🇺🇸 Same 15 Q&A in English
4. Audio Overview script: explain Clean Architecture to a beginner
   🇪🇸 Script del podcast en español
   🇺🇸 Same podcast script in English

---

### SESSION 2 — Application Layer: CQRS, MediatR, Validation
**Goal:** Master the CQRS pattern, MediatR pipeline, FluentValidation behavior.
**Sources:** full-technical-reference.md (Part 2), day-03-application-layer.md,
day-07-crud-put-delete.md, day-08-fluent-validation-error-handler.md, technical-deep-dive.md (section 4)

**Topics:**
- CQRS: Commands (write) vs Queries (read), why separate them
- MediatR: ISender, IRequest<T>, IRequestHandler<TRequest,TResponse>
- Pipeline Behavior: IPipelineBehavior<TRequest,TResponse>, execution order
- ValidationBehavior: intercepts every command, runs validators, throws ValidationException
- FluentValidation: RuleFor, NotEmpty, MaximumLength, Must, WithMessage
- All Commands: Create, UpdateStatus, Edit, Delete, Register, Login, RefreshToken, Logout, ChangePassword
- GetJobApplicationsQuery + Handler: repository call, DTO mapping
- JobApplicationDto: why Status is int (not string), UpdatedAt for stale detection
- C# keywords: record, abstract, generic constraints (where TRequest : IRequest)

**NotebookLM tools — generate each tool TWICE (🇪🇸 then 🇺🇸):**
1. Study Guide: "CQRS + MediatR Pipeline"
   🇪🇸 Guía de estudio en español
   🇺🇸 Same guide in English
2. FAQ: 15 questions on Commands vs Queries, pipeline behaviors, validators
   🇪🇸 15 preguntas en español
   🇺🇸 Same 15 questions in English
3. Briefing Doc: "How a POST /api/jobapplications request flows through the Application layer"
   🇪🇸 Documento en español
   🇺🇸 Same document in English
4. Practice Quiz: 10 multiple-choice questions on CQRS
   🇪🇸 Quiz en español
   🇺🇸 Same quiz in English

---

### SESSION 3 — Infrastructure: EF Core, JWT, Redis, Email, BackgroundService
**Goal:** Understand persistence, authentication infrastructure, caching, and background jobs.
**Sources:** full-technical-reference.md (Part 3), day-04-infrastructure-repositories.md,
day-06-docker-postgresql-migrations.md, day-09-jwt-auth-register-login.md,
day-10-jwt-refresh-token-authorize.md, day-12-serilog-logging.md, day-16-redis-cache.md,
day-26-v1.1-improvements.md

**Topics:**
- ApplicationDbContext: OnModelCreating, OwnsOne (TechStack), HasConversion (enums), table/column names
- EF Core migrations: InitialCreate, AddRefreshTokens — what each adds to the schema
- Repository pattern: IJobApplicationRepository implementation, Include() eager loading
- Unit of Work: IUnitOfWork.SaveChangesAsync(), why it wraps DbContext.SaveChanges
- JWT: header.payload.signature, claims (sub, email, jti), 60-min access token, 7-day refresh token
- JwtTokenGenerator: CreateToken, ValidateToken (ValidateLifetime=false for expired tokens)
- BCrypt: one-way hashing, salt rounds, VerifyHashedPassword
- Refresh token rotation: why tokens are revoked on use (prevent replay attacks)
- Redis cache-aside: GetAsync, SetAsync, RemoveAsync, 10-min TTL
- RedisCacheService: try/catch fallback (critical — Redis not in production yet)
- StaleNotificationService: BackgroundService, IServiceScopeFactory, 24h interval, SMTP email
- Serilog: structured logging, rolling files, MinimumLevel configuration
- DependencyInjection.cs: AddInfrastructure extension method, service lifetimes (Singleton, Scoped, Transient)

**NotebookLM tools — generate each tool TWICE (🇪🇸 then 🇺🇸):**
1. Study Guide: "Infrastructure Layer — EF Core, JWT, Redis"
   🇪🇸 Guía de estudio en español
   🇺🇸 Same guide in English
2. Mind Map: JWT token lifecycle (register → login → access token → 401 → refresh → retry → logout)
   🇪🇸 Mapa mental en español
   🇺🇸 Same mind map in English
3. FAQ: 20 questions on EF Core, JWT, Redis, BackgroundService
   🇪🇸 20 preguntas en español
   🇺🇸 Same 20 questions in English
4. Audio Overview script: "How JWT authentication works in this project"
   🇪🇸 Script del podcast en español
   🇺🇸 Same podcast script in English

---

### SESSION 4 — API Layer: Controllers, Middleware, Program.cs
**Goal:** Master the HTTP pipeline, all endpoints, error handling, rate limiting, CORS.
**Sources:** full-technical-reference.md (Part 4), day-05-controller-swagger.md,
day-07-crud-put-delete.md, day-08-fluent-validation-error-handler.md,
day-24-production-fixes.md, day-27-ci-fixes.md, technical-deep-dive.md (section 6)

**Topics:**
- Program.cs middleware pipeline: order matters (CORS → Auth → RateLimit → Routing → Controllers)
- WebApplication builder: AddControllers, AddSwaggerGen, AddMediatR, AddFluentValidation, AddRateLimiter
- ExceptionHandlingMiddleware: maps exceptions to HTTP codes (ValidationException→400, UnauthorizedAccessException→401, KeyNotFoundException→404, Exception→500)
- CORS policy "Frontend": allowed origins (localhost:5173, localhost:5174, ramiro671.github.io)
- Rate limiting: fixed-window "auth" (10 req/min), disabled in Testing environment
- [Authorize] attribute: requires valid JWT, returns 401 otherwise
- All 5 JobApplications endpoints: GET, POST, PUT, PATCH, DELETE — request/response JSON, HTTP codes
- Auth endpoints: /register, /login, /refresh, /logout
- Users endpoints: PUT /api/users/me/password
- Swagger: Bearer token configuration, XML comments
- PATCH vs PUT: why they're separate (different domain operations)

**NotebookLM tools — generate each tool TWICE (🇪🇸 then 🇺🇸):**
1. Study Guide: "ASP.NET Core API Layer"
   🇪🇸 Guía de estudio en español
   🇺🇸 Same guide in English
2. FAQ: 20 questions on middleware, controllers, HTTP methods, CORS
   🇪🇸 20 preguntas en español
   🇺🇸 Same 20 questions in English
3. Briefing Doc: Complete API endpoint reference (all routes, request JSON, response JSON, HTTP codes)
   🇪🇸 Referencia en español
   🇺🇸 Same reference in English
4. Practice Quiz: "Which HTTP status code does each scenario return?"
   🇪🇸 Quiz en español
   🇺🇸 Same quiz in English

---

### SESSION 5 — Frontend: React 18 + TypeScript + Tailwind CSS
**Goal:** Master the entire frontend architecture, all components, and the token refresh pattern.
**Sources:** full-technical-reference.md (Part 5), day-18-21-react-frontend.md,
day-26-v1.1-improvements.md, technical-deep-dive.md (section 7)

**Topics:**
- TypeScript: interfaces, type aliases, as const, union types, optional properties (?)
- ApplicationStatus const (frontend): why values differ from backend enum (intentional mismatch)
- React Context API: createContext, useContext, Provider pattern — AuthContext, ThemeContext, ToastContext
- React hooks: useState, useEffect, useMemo, useCallback, useRef
- AuthContext: JWT in localStorage, userId, login(), logout()
- ThemeContext: class-based dark mode, document.documentElement.classList, localStorage persistence
- ProtectedRoute: wraps authenticated pages, redirects to /login if no token
- BrowserRouter: basename="/jobtracker-pro" for GitHub Pages sub-path
- DashboardPage: useMemo for filtering (search + status), pagination (12/page), stale detection (7 days)
- JobApplicationCard: status badge colors, inline status dropdown, edit/delete buttons
- AddApplicationModal, EditApplicationModal: controlled form inputs, API calls
- Axios interceptor: 401 → queue concurrent requests → refresh token → retry all (queue pattern)
- CSV export: Blob + createObjectURL + RFC 4180 escaping (zero dependencies)
- Tailwind CSS v4: @import "tailwindcss", @variant dark, dark: prefix, custom CSS variables
- Vite: base config '/jobtracker-pro/', VITE_API_URL env variable injection at build time

**NotebookLM tools — generate each tool TWICE (🇪🇸 then 🇺🇸):**
1. Study Guide: "React 18 + TypeScript Frontend"
   🇪🇸 Guía de estudio en español
   🇺🇸 Same guide in English
2. Mind Map: component tree (App → BrowserRouter → Providers → Pages → Components)
   🇪🇸 Mapa mental en español
   🇺🇸 Same mind map in English
3. FAQ: 20 questions on React hooks, Context, TypeScript, Tailwind, Axios interceptor
   🇪🇸 20 preguntas en español
   🇺🇸 Same 20 questions in English
4. Audio Overview script: "How the Axios token refresh interceptor works"
   🇪🇸 Script del podcast en español
   🇺🇸 Same podcast script in English

---

### SESSION 6 — Testing: Unit + Integration (26 tests)
**Goal:** Understand both test types, the testing pyramid, and what each test verifies.
**Sources:** full-technical-reference.md (Part 6), day-13-unit-tests-bloque16.md,
day-14-integration-tests.md, day-27-ci-fixes.md, technical-deep-dive.md (section 8)

**Topics:**
- Testing pyramid: unit → integration → e2e
- xUnit: [Fact], [Theory], [InlineData], [Collection("Integration")]
- Moq: Mock<T>, Setup(), Returns(), Verify() — mock repositories in unit tests
- FluentAssertions: .Should().Be(), .BeGreaterThan(), .NotBeNull(), .Throw<>()
- Unit tests (10): JobApplicationTests (domain entity behavior), CreateJobApplicationHandlerTests (handler with mocked repo)
- Integration tests (16): CustomWebApplicationFactory, UseEnvironment("Testing"), InMemory EF Core
- BaseIntegrationTest: HttpClient, GetAuthTokenAsync (register + login flow)
- Why rate limiter is disabled in Testing: shared IP in WebApplicationFactory → 10 req/min limit
- Why migrations are skipped in Testing: IsEnvironment("Testing") guard in Program.cs
- JobApplicationsControllerTests: 12 tests — Create (valid/invalid), GetAll, UpdateStatus, Edit, Delete (valid/unknown)
- AuthControllerTests: 4 tests — Register (valid/duplicate), Login (valid/wrong password)
- What 400 vs 404 vs 204 vs 201 each mean in the test assertions

**NotebookLM tools — generate each tool TWICE (🇪🇸 then 🇺🇸):**
1. Study Guide: "Testing Strategy — Unit + Integration"
   🇪🇸 Guía de estudio en español
   🇺🇸 Same guide in English
2. FAQ: 15 questions on xUnit, Moq, WebApplicationFactory, InMemory EF
   🇪🇸 15 preguntas en español
   🇺🇸 Same 15 questions in English
3. Briefing Doc: "What each of the 26 tests verifies" (test name + assertion + why it matters)
   🇪🇸 Documento en español
   🇺🇸 Same document in English
4. Practice Quiz: "Is this a unit test or integration test? Why?"
   🇪🇸 Quiz en español
   🇺🇸 Same quiz in English

---

### SESSION 7 — DevOps: CI/CD, Docker, Azure, GitHub Pages
**Goal:** Master the full deployment pipeline from git push to production.
**Sources:** full-technical-reference.md (Part 7, Part 8), day-15-docker-multistage.md,
day-17-github-actions-ci.md, day-22-cicd-azure-netlify.md, day-27-ci-fixes.md,
technical-deep-dive.md (sections 9-11), CHANGELOG.md

**Topics:**
- GitHub Actions: workflow triggers (push/PR to main), jobs, steps, needs (dependency chain)
- CI pipeline: build-and-test → docker-build → deploy-api → deploy-frontend
- Secrets: AZURE_APP_NAME, AZURE_PUBLISH_PROFILE, VITE_API_URL, GITHUB_TOKEN (built-in)
- FORCE_JAVASCRIPT_ACTIONS_TO_NODE24: why added (Node.js 20 deprecation, June 2026 deadline)
- Docker multi-stage build: SDK stage (restore+build+publish) → runtime stage (copy+run)
- Non-root Docker user: why it matters for security
- Azure App Service: dotnet publish → azure/webapps-deploy@v3, connection string type "Custom"
- Neon.tech PostgreSQL: why chosen over Azure DB ($0 vs $60/month), Npgsql connection string format
- GitHub Pages: peaceiris/actions-gh-pages@v4, gh-pages branch, 404.html SPA redirect trick
- SPA routing on GitHub Pages: 404.html encodes path → index.html decodes → React Router renders
- vite.config.ts base: '/jobtracker-pro/' — why needed for sub-path hosting
- Environment variables: VITE_ prefix for frontend (build-time injection), appsettings.json for backend
- Azure double-underscore convention: JwtSettings__Secret for nested config keys

**NotebookLM tools — generate each tool TWICE (🇪🇸 then 🇺🇸):**
1. Study Guide: "CI/CD Pipeline + DevOps"
   🇪🇸 Guía de estudio en español
   🇺🇸 Same guide in English
2. Mind Map: deployment flow (git push → GitHub Actions jobs → Azure API + GitHub Pages frontend)
   🇪🇸 Mapa mental en español
   🇺🇸 Same mind map in English
3. FAQ: 15 questions on Docker, CI/CD, GitHub Pages, Azure, environment variables
   🇪🇸 15 preguntas en español
   🇺🇸 Same 15 questions in English
4. Audio Overview script: "Full deployment pipeline walkthrough"
   🇪🇸 Script del podcast en español
   🇺🇸 Same podcast script in English

---

### SESSION 8 — Integration: Full Request Flow + Practice Feature + Interview Prep
**Goal:** Connect all layers end-to-end, practice implementing a feature, prepare for interviews.
**Sources:** jobtracker-pro-study-guide.md, interview-prep.md, practica-feature-job-description.md,
bloque-33-linkedin-portfolio.md, CHANGELOG.md, interactive-guide.html (open in browser)

**Topics:**
- Full request flow (13 steps): Browser → React → Axios → CORS → Auth → RateLimiter →
  Controller → MediatR → ValidationBehavior → Handler → Repository → EF Core → PostgreSQL →
  Response flows back through each layer
- Practice feature (20-step guide): Job Description + TechStack surface
  - Why no migration needed (all columns already exist in InitialCreate)
  - Steps: DTO → Query → Create/Edit commands → Controller → TypeScript types → Frontend → Tests
  - Git workflow: feature branch → PR → squash merge → deploy
- Common bugs and fixes: CS7036 (missing required member), NullRef on TechStack, 400 from PATCH, TS2345
- JUNIOR interview questions (20): Clean Architecture basics, REST, CRUD, JWT, React hooks
- MID interview questions (20): CQRS, pipeline behaviors, EF Core owned entities, token refresh queue
- SENIOR interview questions (20): architectural trade-offs, testing strategy, CI/CD design decisions
- 30-question practice exam with answer key
- LinkedIn elevator pitch, GitHub README, portfolio talking points

**NotebookLM tools — generate each tool TWICE (🇪🇸 then 🇺🇸):**
1. Study Guide: "Full Request Flow + Interview Preparation"
   🇪🇸 Guía de estudio en español
   🇺🇸 Same guide in English
2. FAQ: 20 interview questions (JUNIOR + MID + SENIOR) with detailed answers
   🇪🇸 20 preguntas de entrevista en español
   🇺🇸 Same 20 interview questions in English
3. Practice Exam: 30 questions with answer key
   🇪🇸 Examen de práctica en español
   🇺🇸 Same exam in English
4. Audio Overview script (Podcast): "Explain this project to a hiring manager"
   🇪🇸 Script del podcast en español — para entender los conceptos
   🇺🇸 Same podcast script in English — para practicar el inglés técnico

---

## ADDITIONAL INSTRUCTIONS FOR NOTEBOOKLM

### BILINGUAL FORMAT
Always present content in this exact format:

  🇪🇸 ESPAÑOL
  [Full content in Spanish]

  ---

  🇺🇸 ENGLISH
  [Exact same content in English]

The English version must be a direct translation of the Spanish — not a summary, not simplified.
Identical structure, identical examples, identical code references.

### TECHNICAL ACCURACY
1. **Prioritize full-technical-reference.md** for all code-level questions (2,172 lines of
   documented code, the most complete source).
2. **Use jobtracker-pro-study-guide.md** for pre-built Q&A, flashcards, and the practice exam.
3. **Never mix sessions** — each session covers distinct topics, no repetition.
4. For every concept, always explain:
   (a) What it is in plain language
   (b) The C#/.NET or TypeScript keyword/syntax
   (c) How it's used specifically in JobTracker Pro with a code example

### VERSION AWARENESS
- .NET 10.0 / ASP.NET Core 10.0
- React 18.3.1 / TypeScript 5.6.2 / Vite 6.0.5
- Tailwind CSS 4.0 / EF Core 10.0
- MediatR 12.4.1 / FluentValidation 11.11.0 / xUnit 2.9.2

### CRITICAL FACTS — never get these wrong
- Tests: 26 total (10 unit + 16 integration). Never say 22.
- Frontend URL: https://ramiro671.github.io/jobtracker-pro/ (GitHub Pages, NOT Netlify)
- Status enum mismatch: Backend Applied=1, Frontend Applied=0. INTENTIONAL. DTO returns int.
- Rate limiter: disabled in Testing environment (IsEnvironment check in Program.cs)
- Migrations: skipped in Testing environment (same IsEnvironment check)

### ENGLISH PRACTICE TIPS
When generating the English version, use proper technical English vocabulary:
- "implements" (not "use")
- "returns" (not "give back")
- "throws an exception" (not "send error")
- "dependency injection" (not "inject")
- "asynchronous" (not "async in Spanish")
- "middleware pipeline" (not "middleware chain")
Include a VOCABULARY section at the end of each English FAQ listing 10 key technical terms
from that session with their pronunciation guide.

### EXECUTION ORDER — GENERATE ALL 8 SESSIONS NOW
Do NOT wait for further instructions. Do NOT ask for confirmation between sessions.
Generate ALL 8 sessions sequentially and completely in a single response, following this order:

  SESSION 1 → SESSION 2 → SESSION 3 → SESSION 4 → SESSION 5 → SESSION 6 → SESSION 7 → SESSION 8

For each session:
  1. Print the session title and goal
  2. Generate every tool listed (Study Guide, Mind Map or Briefing Doc, FAQ, Audio script, Quiz/Exam)
  3. Each tool must appear TWICE: 🇪🇸 ESPAÑOL first, then 🇺🇸 ENGLISH
  4. Separate sessions with a clear divider: ══════════════════════════════════

Begin immediately with SESSION 1. Do not stop until SESSION 8 is complete.
```
