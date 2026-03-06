# Contexto del Proyecto — Ramiro López

## Quién soy
- Nombre: Ramiro López
- Nivel: Mid-Level High / Senior .NET Developer
- Objetivo: Conseguir trabajo en empresa internacional en inglés como Senior Developer
- Plazo: Marzo → Julio 2026
- Ubicación: México City (GMT-6)

---

## Proyecto principal: JobTracker Pro

**Qué es:** Aplicación web full-stack para rastrear postulaciones de trabajo.  
**Repo:** `C:\Users\ramir\Source\repos\jobtracker-pro` (GitHub: jobtracker-pro)  
**Referencia de código existente:** `C:\Users\ramir\Source\repos\LinkedInAgent.Grpc`  
(1,319 job postings, Medallion Architecture, gRPC, MongoDB, Playwright, OAuth2)

### Stack tecnológico
- **Backend:** ASP.NET Core 8 · C# · Clean Architecture · EF Core · PostgreSQL · JWT · Redis · Serilog
- **Frontend:** React · TypeScript · Vite · Tailwind CSS · React Hook Form · React Router
- **Testing:** xUnit · NUnit · Moq · FluentAssertions · WebApplicationFactory
- **DevOps:** Docker · Docker Compose · GitHub Actions · Azure App Service · Netlify
- **Herramientas:** VS Code · C# Dev Kit · Claude Code v2.1.69 · Node.js v20.16 · Git

### Arquitectura: Clean Architecture
```
JobTrackerPro.sln
├── src/
│   ├── JobTrackerPro.Domain/          # Entidades, interfaces, value objects
│   ├── JobTrackerPro.Application/     # Use cases, DTOs, MediatR handlers
│   ├── JobTrackerPro.Infrastructure/  # EF Core, repositorios, JWT, Redis
│   └── JobTrackerPro.Api/             # Controllers, middleware, DI config
└── tests/
    ├── JobTrackerPro.UnitTests/
    └── JobTrackerPro.IntegrationTests/
```

### Reglas de código (SIEMPRE)
- Variables, métodos, clases: **en inglés** (`JobOffer`, no `OfertaLaboral`)
- XML comments en inglés: `/// <summary>Gets all job offers</summary>`
- Commits en inglés con Conventional Commits:
  - `feat:` · `fix:` · `docs:` · `chore:` · `test:` · `refactor:` · `ci:`
- README.md siempre en inglés
- **NUNCA** hardcodear secrets (usar appsettings + env vars)
- **NUNCA** clases God (separar responsabilidades)
- **SIEMPRE** usar interfaces (no implementación directa)

---

## Plan de 17 semanas (Fases del proyecto)

| Fase | Semanas | Período | Contenido |
|------|---------|---------|-----------|
| Fase 1 | 1-4 | 5 Mar–31 Mar | ASP.NET Core · EF Core · PostgreSQL · JWT · Clean Architecture |
| Fase 2 | 5-7 | 1 Abr–21 Abr | xUnit · NUnit · Moq · Docker · Docker Compose · Redis |
| Fase 3 | 8-11 | 22 Abr–19 May | React · TypeScript · Auth UI · Tailwind CSS |
| Fase 4 | 12-15 | 20 May–16 Jun | GitHub Actions · Azure · Netlify · Application Insights |
| Fase 5 | 16-17 | 17 Jun–4 Jul | Documentación · Portfolio · Mock interviews en inglés |

### Bloques diarios completados
- **Bloque 1:** Setup Clean Architecture (4 proyectos, referencias configuradas)
- (Se irán actualizando aquí)

---

## Plan de inglés: English for IT 1 + 2

### Cursos (Cisco NetAcad / OpenEDG)
- **IT1:** English for IT 1 — B1 → B1+ (50h) — 5 Mar → 20 Abr
- **IT2:** English for IT 2 — B1+ → B2 (50h) — 21 Abr → 4 Jul

### Certificaciones objetivo
- English for IT B1+ / GSE 43-58 (~20 Abr)
- English for IT B2 / GSE 59-75 (~22 Jun) ← nivel mínimo para trabajo en inglés

### Temario English for IT 1
| Módulo | Tema | Gramática principal |
|--------|------|---------------------|
| Mod 1 | Product Management: Identify problems | Passive Voice · Relative Pronouns · Past Simple as if |
| Mod 2 | Network Engineer: Interpret & Implement | Emphatic do · Dependent Prepositions |
| Mod 3 | Software Engineer: Analyze Factual Info | Conjunctions · Inversion · Modal Verbs |
| Mod 4 | Customer Support: Examine Key Info | Adverbs · Idioms |
| Mod 5 | Security Engineer: Incorporate Goals | Imperative · Adverbs of Possibility/Probability |
| Mod 6 | Security Engineer: Participate in Discussion | Future Perfect · Interview simulation |
| App A | Extra Grammar & Glossary | Tenses · Modal Verbs · Adverbs |
| App B | Extra Grammar Exercises | Práctica de todo |

### Temario English for IT 2
| Módulo | Tema | Gramática principal |
|--------|------|---------------------|
| Mod 1 | Network Engineer: Analyze and Prioritize | Subjunctive · Logical Deduction · Since |
| Mod 2 | Customer Support: Request Feedback | Negative Future Tenses · Polite Requests B2 |
| Mod 3 | Software Engineer: Evaluate Information | They singular · Emphasis Yet |
| Mod 4 | Software Engineer: Identify Issues | Phrasal Verbs · Cleft Sentences |
| Mod 5 | Network Engineer: Defend a Point of View | Comparatives While/Whereas · Interview |
| Mod 6 | Product Management: Update Colleagues | Polite Phrasing · Even if/though · Past Perfect |
| App A | Extra Grammar | Conditionals · Prepositions · Gerunds |

### Horario diario
- **17:00 PM** → 🇺🇸 Inglés (1 hora)
- **19:00 PM** → 💻 JobTracker Pro (1 hora)

### Regla de inglés orgánico (todos los días)
- Commits en inglés ✓
- Variables y métodos en inglés ✓
- XML comments en inglés ✓
- README.md en inglés ✓
- Daily log (day-XX.md) en inglés ✓

---

## Logs diarios
**Ubicación:** `C:\Users\ramir\Source\repos\jobtracker-pro\docs\daily-logs\`  
**Formato:** `day-XX-topic.md`  
**Workflow:** Claude genera el archivo → usuario copia → `git commit` → `git push`

### Logs creados
- `day-00-setup.md` ✅ (primer commit al repo)
- `day-00-git-commands.md` ✅

---

## Estado actual (5 Mar 2026 — Día 0 completado)

### ✅ Completado
- Claude Pro configurado
- VS Code + C# Dev Kit + extensiones instaladas
- Claude Code v2.1.69 instalado
- Node.js v20.16 + Git configurado
- Repo `jobtracker-pro` creado en GitHub
- Primer commit push exitoso (git pull --rebase resuelto)
- Análisis de LinkedInAgent.Grpc completado
- Plan de 17 semanas definido
- Calendarios Google Calendar importados (74 eventos)

### 🚀 Siguiente paso (Día 1 — HOY)
- 17:00 → [IT1 1.1] Grammar: The Passive Voice
- 19:00 → [JobTracker] Bloque 1: Setup Clean Architecture
  ```
  dotnet new sln -n JobTrackerPro
  dotnet new webapi -n JobTrackerPro.Api
  dotnet new classlib -n JobTrackerPro.Domain
  dotnet new classlib -n JobTrackerPro.Application
  dotnet new classlib -n JobTrackerPro.Infrastructure
  ```

---

## Errores críticos detectados en LinkedInAgent.Grpc (a NO repetir)
1. **Secrets hardcodeados** → usar appsettings + variables de entorno
2. **Sin interfaces** → siempre programar contra abstracciones
3. **God class** → separar responsabilidades en clases pequeñas

---

## Cómo usar este contexto en nuevas conversaciones
Cuando abras una conversación nueva dentro de este proyecto, Claude ya sabe:
- Quién eres y qué estás construyendo
- En qué fase del proyecto vas
- Qué bloque de inglés estás estudiando
- Las reglas del código
- El stack tecnológico

Simplemente di **"Continuemos con el Bloque X"** o **"Tengo dudas sobre [tema de inglés]"** y Claude retoma desde donde dejaste.
