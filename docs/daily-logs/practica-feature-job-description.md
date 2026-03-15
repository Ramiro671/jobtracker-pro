# Practica Final — Feature: Job Description & Tech Stack

**Objetivo:** Implementar una funcionalidad completa que recorra TODAS las capas del proyecto,
desde el Domain hasta el Frontend, preparando el terreno para la integración con LinkedInAgent.Grpc.

**Estado inicial:** La DB ya tiene las columnas. La entidad ya tiene los campos y métodos.
Tu trabajo es conectar todo hacia arriba.

**Tiempo estimado:** 4-6 horas de trabajo real.

---

## Contexto: Por qué esta feature

El proyecto **LinkedInAgent.Grpc** hace scraping de ofertas de LinkedIn y extrae:
- `RawText` — texto completo del anuncio de trabajo (Bronze layer)
- `Technologies` / `Frameworks` — tech stack analizado por Gemini IA (Silver layer)
- `SeniorityLevel` — nivel de experiencia requerido (Silver layer)

El objetivo futuro es que LinkedInAgent llame a JobTracker Pro via HTTP y cree
aplicaciones con TODA esa información ya rellena automáticamente.

Para que eso sea posible, JobTracker Pro necesita exponer esos campos por su API.

---

## Qué vas a construir

**Feature: "Job Description & Tech Stack"**

Permite guardar y visualizar:
1. **`description`** — el texto completo del anuncio de trabajo (cuadro de texto grande)
2. **`techStack`** — 5 campos: backend, frontend, databases, cloudAndDevOps, testing
3. **`contactName` / `contactEmail`** — datos del recruiter

**Resultado visual esperado:**
- Al abrir "Add Application" o "Edit": aparecen campos nuevos (Description, TechStack, Contact)
- En cada tarjeta: se muestra un mini-badge de tech stack (ej: `C#` `React` `PostgreSQL`)
- En el detalle de una tarjeta: se puede ver la descripción completa

---

## Paso 0 — Crea tu rama de trabajo

```bash
# Siempre trabaja en una rama, nunca directo en main
git checkout main
git pull origin main
git checkout -b feature/job-description-techstack

# Verifica que estás en la nueva rama
git branch
# → * feature/job-description-techstack
#     main
```

---

## Paso 1 — Domain Layer (lectura, no cambios)

El Domain ya está listo. Abre estos archivos y **léelos, no los modifiques**:

```
src/JobTrackerPro.Domain/Entities/JobApplication.cs
src/JobTrackerPro.Domain/ValueObjects/TechStack.cs
src/JobTrackerPro.Domain/Enums/SeniorityLevel.cs
src/JobTrackerPro.Domain/Enums/WorkModality.cs
```

**Preguntas que debes poder responder antes de continuar:**
- ¿Cuáles son los métodos de dominio para actualizar TechStack y Contact?
- ¿Cómo se crea un `TechStack` value object?
- ¿Qué hace `TechStack.FromFlatString(technologies, frameworks)`?
- ¿Por qué `JobApplication` tiene un constructor `private`?
- ¿Qué diferencia hay entre `UpdateDetails()` y `SetTechStack()`?

**Respuestas esperadas:**
```
SetTechStack(TechStack techStack)   ← para TechStack
SetContact(string? name, string? email) ← para ContactName/ContactEmail
TechStack.Create(backend, frontend, databases, cloudAndDevOps, testing)
TechStack.FromFlatString(tech, frameworks) ← para migrar desde el formato flat de Gemini
Constructor privado = EF Core lo usa via reflexión; nadie más puede crear la entidad sin pasar por Create()
UpdateDetails() = título + URL + notes | SetTechStack() = solo tech stack (separación de responsabilidades)
```

---

## Paso 2 — Application Layer: Actualizar el DTO

**Archivo:** `src/JobTrackerPro.Application/DTOs/JobApplicationDto.cs`

El DTO actual ya tiene: `WorkModality`, `SeniorityLevel`, `Source`, `SalaryMin`, `SalaryMax`, `SalaryCurrency`.

**Debes agregar** estos campos al record:

```csharp
// Agrega ANTES del último parámetro (UpdatedAt)
string? Description,
string TechStackBackend,
string TechStackFrontend,
string TechStackDatabases,
string TechStackCloudAndDevOps,
string TechStackTesting,
string? ContactName,
string? ContactEmail,
```

**Resultado final esperado del DTO:**
```csharp
public record JobApplicationDto(
    Guid Id,
    string Title,
    string CompanyName,
    int Status,
    string WorkModality,
    string SeniorityLevel,
    string? JobUrl,
    string? Source,
    decimal? SalaryMin,
    decimal? SalaryMax,
    string? SalaryCurrency,
    string? Description,
    string TechStackBackend,
    string TechStackFrontend,
    string TechStackDatabases,
    string TechStackCloudAndDevOps,
    string TechStackTesting,
    string? ContactName,
    string? ContactEmail,
    string? Notes,
    DateTime CreatedAt,
    DateTime? AppliedAt,
    DateTime? UpdatedAt
);
```

> **Concepto que aprendes aquí:** Por qué TechStack se "aplana" (flatten) en el DTO.
> El Value Object `TechStack` tiene 5 propiedades. En el DTO no metemos el objeto completo
> — lo aplanamos a 5 strings separados. Esto hace el DTO más simple para el frontend (JSON plano).

---

## Paso 3 — Application Layer: Actualizar el Query Handler

**Archivo:** `src/JobTrackerPro.Application/JobApplications/Queries/GetJobApplicationsHandler.cs`

Busca donde se crea el `JobApplicationDto` (el mapping de entidad a DTO).

Actualmente el mapping no incluye los campos nuevos. Debes agregar el mapeo.

**Pista:** la entidad `JobApplication` tiene `app.TechStack.Backend`, `app.TechStack.Frontend`, etc.

**Código que debes agregar al mapping:**
```csharp
Description: app.Description,
TechStackBackend: app.TechStack.Backend,
TechStackFrontend: app.TechStack.Frontend,
TechStackDatabases: app.TechStack.Databases,
TechStackCloudAndDevOps: app.TechStack.CloudAndDevOps,
TechStackTesting: app.TechStack.Testing,
ContactName: app.ContactName,
ContactEmail: app.ContactEmail,
```

> **Concepto:** El Handler es el único lugar que sabe tanto del Domain como del DTO.
> La entidad nunca se expone directamente — siempre se mapea.

---

## Paso 4 — Application Layer: Actualizar CreateJobApplicationCommand

**Archivo:** `src/JobTrackerPro.Application/JobApplications/Commands/CreateJobApplicationCommand.cs`

Agrega los nuevos campos al record de comando:

```csharp
public record CreateJobApplicationCommand(
    Guid UserId,
    string Title,
    string CompanyName,
    string? JobUrl = null,
    string? Description = null,        // NUEVO
    string? Source = null,
    string TechStackBackend = "",      // NUEVO
    string TechStackFrontend = "",     // NUEVO
    string TechStackDatabases = "",    // NUEVO
    string TechStackCloudAndDevOps = "",// NUEVO
    string TechStackTesting = ""       // NUEVO
    // Nota: SeniorityLevel, WorkModality, Salary, Contact son para el EditCommand
) : IRequest<JobApplicationDto>;
```

> **Por qué no ponemos TODOS los campos en Create?**
> El formulario de "Add Application" captura lo mínimo para empezar a trackear.
> Los detalles enriquecidos (TechStack completo, Contact, Salary) se agregan via Edit.
> Pero para el flujo de LinkedInAgent (auto-fill) sí queremos Description y TechStack en Create.

---

## Paso 5 — Application Layer: Actualizar CreateJobApplicationHandler

**Archivo:** `src/JobTrackerPro.Application/JobApplications/Commands/CreateJobApplicationHandler.cs`

Después de crear la aplicación con `JobApplication.Create(...)`, agrega:

```csharp
// Si viene con tech stack, lo seteamos inmediatamente
if (!string.IsNullOrWhiteSpace(request.TechStackBackend) ||
    !string.IsNullOrWhiteSpace(request.TechStackFrontend))
{
    var techStack = TechStack.Create(
        backend: request.TechStackBackend,
        frontend: request.TechStackFrontend,
        databases: request.TechStackDatabases,
        cloudAndDevOps: request.TechStackCloudAndDevOps,
        testing: request.TechStackTesting
    );
    application.SetTechStack(techStack);
}
```

---

## Paso 6 — Application Layer: Actualizar EditJobApplicationCommand

**Archivo:** `src/JobTrackerPro.Application/JobApplications/Commands/EditJobApplicationCommand.cs`

El comando PATCH (`EditJobApplicationCommand`) edita los campos mutables.
Agrega los nuevos campos:

```csharp
public record EditJobApplicationCommand(
    Guid Id,
    string Title,
    string? JobUrl,
    string? Notes,
    string? Description,           // NUEVO
    string TechStackBackend = "",  // NUEVO
    string TechStackFrontend = "", // NUEVO
    string TechStackDatabases = "",// NUEVO
    string TechStackCloudAndDevOps = "",// NUEVO
    string TechStackTesting = "",  // NUEVO
    string? ContactName = null,    // NUEVO
    string? ContactEmail = null    // NUEVO
) : IRequest<bool>;
```

---

## Paso 7 — Application Layer: Actualizar EditJobApplicationHandler

**Archivo:** `src/JobTrackerPro.Application/JobApplications/Commands/EditJobApplicationHandler.cs`

Después de `application.UpdateDetails(...)`, agrega:

```csharp
// Actualiza TechStack
var techStack = TechStack.Create(
    backend: request.TechStackBackend,
    frontend: request.TechStackFrontend,
    databases: request.TechStackDatabases,
    cloudAndDevOps: request.TechStackCloudAndDevOps,
    testing: request.TechStackTesting
);
application.SetTechStack(techStack);

// Actualiza contacto del recruiter
application.SetContact(request.ContactName, request.ContactEmail);
```

---

## Paso 8 — API Layer: Actualizar los Request models

**Archivo:** `src/JobTrackerPro.Api/Controllers/JobApplicationsController.cs`

Busca los records `CreateJobApplicationRequest` y `EditJobApplicationRequest`
(o `EditJobApplicationCommand` si se usa directamente desde el body).

Agrega los nuevos campos en el request body:

```csharp
public record CreateJobApplicationRequest(
    Guid UserId,
    string Title,
    string CompanyName,
    string? JobUrl,
    string? Description,        // NUEVO
    string? Source,
    string TechStackBackend = "",
    string TechStackFrontend = "",
    string TechStackDatabases = "",
    string TechStackCloudAndDevOps = "",
    string TechStackTesting = ""
);
```

---

## Paso 9 — Verificar que el backend compila

```bash
# Desde la raíz del repo
dotnet build --configuration Release

# Si hay errores:
# - Revisa que el record del DTO tenga todos los parámetros en el mismo orden
# - Revisa que el mapping en el Handler incluya los nuevos campos
# - Revisa que no tengas un , faltante o extra en el record
```

**Sin errores = puedes pasar al frontend.**

---

## Paso 10 — Frontend: Actualizar los tipos TypeScript

**Archivo:** `frontend/src/types/index.ts`

Agrega los nuevos campos a la interfaz `JobApplication`:

```typescript
export interface JobApplication {
  id: string;
  title: string;
  companyName: string;
  status: number;
  workModality: string;
  seniorityLevel: string;
  jobUrl?: string;
  source?: string;
  salaryMin?: number;
  salaryMax?: number;
  salaryCurrency?: string;
  description?: string;          // NUEVO
  techStackBackend: string;      // NUEVO
  techStackFrontend: string;     // NUEVO
  techStackDatabases: string;    // NUEVO
  techStackCloudAndDevOps: string;// NUEVO
  techStackTesting: string;      // NUEVO
  contactName?: string;          // NUEVO
  contactEmail?: string;         // NUEVO
  notes?: string;
  createdAt: string;
  appliedAt?: string;
  updatedAt?: string;
}
```

---

## Paso 11 — Frontend: Actualizar AddApplicationModal

**Archivo:** `frontend/src/components/AddApplicationModal.tsx`

Agrega el campo `description` con un `<textarea>` grande y los campos de TechStack:

```tsx
{/* Description — el texto completo del anuncio */}
<div>
  <label className={labelCls}>Job Description</label>
  <textarea
    className={`${inputCls} h-40 resize-y`}
    placeholder="Paste the full job description here..."
    value={description}
    onChange={e => setDescription(e.target.value)}
  />
  <p className="text-xs text-gray-400 mt-1">
    Optional — paste the LinkedIn job post. This will be used for AI analysis.
  </p>
</div>

{/* Tech Stack — sección expandible */}
<details className="group">
  <summary className="cursor-pointer text-sm font-medium text-gray-300
                       hover:text-white list-none flex items-center gap-2">
    <span className="text-gray-400 group-open:rotate-90 transition-transform">▶</span>
    Tech Stack (optional)
  </summary>
  <div className="mt-3 space-y-3 pl-4 border-l border-gray-600">
    <div>
      <label className={labelCls}>Backend</label>
      <input className={inputCls} placeholder="C#, .NET Core, ASP.NET Core"
             value={techStackBackend} onChange={e => setTechStackBackend(e.target.value)} />
    </div>
    <div>
      <label className={labelCls}>Frontend</label>
      <input className={inputCls} placeholder="React, TypeScript, Tailwind"
             value={techStackFrontend} onChange={e => setTechStackFrontend(e.target.value)} />
    </div>
    <div>
      <label className={labelCls}>Databases</label>
      <input className={inputCls} placeholder="PostgreSQL, Redis, MongoDB"
             value={techStackDatabases} onChange={e => setTechStackDatabases(e.target.value)} />
    </div>
    <div>
      <label className={labelCls}>Cloud & DevOps</label>
      <input className={inputCls} placeholder="Azure, Docker, GitHub Actions"
             value={techStackCloudDevops} onChange={e => setTechStackCloudDevops(e.target.value)} />
    </div>
    <div>
      <label className={labelCls}>Testing</label>
      <input className={inputCls} placeholder="xUnit, Playwright, Cypress"
             value={techStackTesting} onChange={e => setTechStackTesting(e.target.value)} />
    </div>
  </div>
</details>
```

---

## Paso 12 — Frontend: Actualizar JobApplicationCard

**Archivo:** `frontend/src/components/JobApplicationCard.tsx`

Agrega un mini tech-stack display debajo del status badge:

```tsx
{/* Tech Stack mini display */}
{(app.techStackBackend || app.techStackFrontend) && (
  <div className="flex flex-wrap gap-1 mt-2">
    {app.techStackBackend.split(',').slice(0, 3).map(t => t.trim()).filter(Boolean).map(tech => (
      <span key={tech}
            className="text-xs bg-blue-900/40 text-blue-300
                       border border-blue-700 rounded px-1.5 py-0.5">
        {tech}
      </span>
    ))}
    {app.techStackFrontend.split(',').slice(0, 2).map(t => t.trim()).filter(Boolean).map(tech => (
      <span key={tech}
            className="text-xs bg-cyan-900/40 text-cyan-300
                       border border-cyan-700 rounded px-1.5 py-0.5">
        {tech}
      </span>
    ))}
  </div>
)}

{/* Description preview — solo primeras 2 líneas */}
{app.description && (
  <p className="text-xs text-gray-400 mt-2 line-clamp-2">{app.description}</p>
)}
```

---

## Paso 13 — Tests: Agregar prueba de integración para el nuevo campo

**Archivo:** `tests/JobTrackerPro.IntegrationTests/JobApplicationsControllerTests.cs`

Agrega este test:

```csharp
[Fact]
public async Task CreateJobApplication_WithDescription_ShouldPersistAndReturnDescription()
{
    // Arrange
    var (client, userId) = await CreateAuthenticatedClientAsync();

    var request = new
    {
        UserId = userId,
        Title = "Senior .NET Developer",
        CompanyName = "Acme Corp",
        Description = "We are looking for a .NET developer with 5+ years experience.",
        TechStackBackend = "C#, ASP.NET Core, Entity Framework",
        TechStackFrontend = "React, TypeScript",
        TechStackDatabases = "PostgreSQL, Redis"
    };

    // Act
    var response = await client.PostAsJsonAsync("/api/jobapplications", request);
    response.EnsureSuccessStatusCode();

    var created = await response.Content.ReadFromJsonAsync<JobApplicationDto>();

    // Assert
    Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    Assert.Equal("We are looking for a .NET developer with 5+ years experience.",
                 created!.Description);
    Assert.Equal("C#, ASP.NET Core, Entity Framework", created.TechStackBackend);
    Assert.Equal("React, TypeScript", created.TechStackFrontend);
}
```

> **Concepto:** Un integration test no usa mocks — golpea el endpoint real con la DB en memoria
> (EF Core InMemory). Verifica que TODO el pipeline funcione: Controller → MediatR → Handler
> → Repository → EF Core → Response.

---

## Paso 14 — Agregar unit test para SetTechStack

**Archivo:** `tests/JobTrackerPro.UnitTests/JobApplicationTests.cs`

```csharp
[Fact]
public void SetTechStack_ShouldUpdateTechStackAndUpdatedAt()
{
    // Arrange
    var app = JobApplication.Create(
        userId: Guid.NewGuid(),
        title: "Test Job",
        companyId: Guid.NewGuid()
    );

    var techStack = TechStack.Create(
        backend: "C#, .NET Core",
        frontend: "React",
        databases: "PostgreSQL"
    );

    // Act
    var before = app.UpdatedAt;
    app.SetTechStack(techStack);

    // Assert
    Assert.Equal("C#, .NET Core", app.TechStack.Backend);
    Assert.Equal("React", app.TechStack.Frontend);
    Assert.Equal("PostgreSQL", app.TechStack.Databases);
    Assert.NotNull(app.UpdatedAt);
    Assert.True(app.UpdatedAt > before);   // UpdatedAt fue actualizado
}
```

---

## Paso 15 — Ejecutar todos los tests

```bash
dotnet test

# Resultado esperado:
# Passed:  24+ tests
# Failed:  0
# Skipped: 0
```

Si algún test falla, lee el mensaje de error — te dirá exactamente qué falló y en qué línea.

---

## Paso 16 — Commit a tu rama

```bash
# Ve qué archivos cambiaste
git status

# Agrega los archivos relevantes (uno por uno, no git add -A)
git add src/JobTrackerPro.Application/DTOs/JobApplicationDto.cs
git add src/JobTrackerPro.Application/JobApplications/Queries/GetJobApplicationsHandler.cs
git add src/JobTrackerPro.Application/JobApplications/Commands/CreateJobApplicationCommand.cs
git add src/JobTrackerPro.Application/JobApplications/Commands/CreateJobApplicationHandler.cs
git add src/JobTrackerPro.Application/JobApplications/Commands/EditJobApplicationCommand.cs
git add src/JobTrackerPro.Application/JobApplications/Commands/EditJobApplicationHandler.cs
git add src/JobTrackerPro.Api/Controllers/JobApplicationsController.cs
git add frontend/src/types/index.ts
git add frontend/src/components/AddApplicationModal.tsx
git add frontend/src/components/EditApplicationModal.tsx
git add frontend/src/components/JobApplicationCard.tsx
git add tests/JobTrackerPro.IntegrationTests/JobApplicationsControllerTests.cs
git add tests/JobTrackerPro.UnitTests/JobApplicationTests.cs

# Crea el commit
git commit -m "feat: surface job description, tech stack, and contact fields through API and frontend

- JobApplicationDto: add Description, TechStack* (5 fields), ContactName, ContactEmail
- GetJobApplicationsHandler: map new entity fields to DTO
- CreateJobApplicationCommand/Handler: accept Description and TechStack on create
- EditJobApplicationCommand/Handler: accept Description, TechStack, Contact on edit
- JobApplicationsController: expose new fields in request bodies
- AddApplicationModal: Description textarea + collapsible TechStack section
- EditApplicationModal: same fields for editing
- JobApplicationCard: mini tech stack badges + description preview
- Tests: +2 (integration: description persists; unit: SetTechStack updates entity)"
```

---

## Paso 17 — Subir la rama a GitHub

```bash
# Primera vez: -u establece el upstream (tracking)
git push -u origin feature/job-description-techstack

# La salida te dará la URL para crear el PR:
# remote: Create a pull request for 'feature/job-description-techstack' on GitHub by visiting:
# remote:   https://github.com/Ramiro671/jobtracker-pro/pull/new/feature/job-description-techstack
```

---

## Paso 18 — Crear el Pull Request (PR)

```bash
# Con GitHub CLI (gh):
gh pr create \
  --title "feat: job description, tech stack, and contact fields" \
  --body "## Summary
- Surfaces the 8 DB columns that were already in the schema but not exposed through the API
- Description field (textarea) allows pasting the full job posting
- Tech Stack section (5 fields) enables tracking required technologies
- Contact fields for recruiter name and email
- Prepares JobTracker Pro for LinkedInAgent.Grpc auto-fill integration

## Test plan
- [ ] dotnet test passes (all 24+ tests green)
- [ ] Add Application modal shows Description and Tech Stack fields
- [ ] Edit Application modal shows the same fields pre-filled
- [ ] JobApplicationCard shows tech stack mini-badges
- [ ] New integration test: description persists through full pipeline
- [ ] New unit test: SetTechStack updates entity correctly" \
  --base main \
  --head feature/job-description-techstack
```

---

## Paso 19 — Merge a main (después de que el PR pasa CI)

El pipeline de CI corre automáticamente cuando abres el PR:
1. `build-and-test` → todos los tests deben estar en verde
2. `docker-build` → la imagen debe compilar
3. Si todo pasa → puedes hacer merge

```bash
# Opción A: desde la UI de GitHub (botón "Merge pull request")

# Opción B: con GitHub CLI
gh pr merge --squash --delete-branch

# Después del merge, actualiza tu local:
git checkout main
git pull origin main

# Borra la rama local (ya fue mergeada)
git branch -d feature/job-description-techstack
```

---

## Paso 20 — Deploy automático

Después del merge a `main`, GitHub Actions corre automáticamente:

```
git merge feature → main
        ↓
1. build-and-test      ← verifica que todo compila y tests pasan
2. docker-build        ← valida imagen Docker
3. deploy-api          ← Azure App Service se actualiza en ~2 min
4. deploy-frontend     ← Netlify se actualiza en ~1 min
```

Puedes monitorear en:
```
https://github.com/Ramiro671/jobtracker-pro/actions
```

---

## Guia de depuracion para errores comunes

### "CS7036: There is no argument given that corresponds to parameter..."
```
CAUSA: Agregaste un campo al record DTO pero no lo incluiste en el mapping del Handler.
SOLUCION: Busca todos los lugares donde se crea JobApplicationDto y agrega el campo faltante.
```

### "NullReferenceException in app.TechStack.Backend"
```
CAUSA: app.TechStack es null (la entidad no tiene el value object inicializado).
SOLUCION: En JobApplication.Create(), verifica que TechStack = TechStack.Create() por defecto.
  La entidad lo tiene: public TechStack TechStack { get; private set; } = TechStack.Create();
```

### "400 Bad Request al hacer POST desde el frontend"
```
CAUSA: El JSON enviado no coincide con lo que espera el controller.
SOLUCION: Abre DevTools → Network → selecciona el request → mira el Request Body y el Response.
  El body del response 400 tendrá los campos de validación que fallaron.
```

### "TS2345: Argument of type... is not assignable to parameter..."
```
CAUSA: La interfaz TypeScript JobApplication no tiene el campo nuevo.
SOLUCION: Agrega el campo a la interfaz en frontend/src/types/index.ts
```

### "Test failed: Expected '...' but got ''"
```
CAUSA: El handler no mapea el campo nuevo al DTO.
SOLUCION: Busca en GetJobApplicationsHandler.cs donde se crea JobApplicationDto y agrega el campo.
```

---

## Conexion futura con LinkedInAgent.Grpc

Una vez que esta feature esté deployada, LinkedInAgent.Grpc puede llamar a JobTracker Pro:

```
LinkedInAgent.Grpc (C#)
  1. Lee Gmail → extrae URLs de LinkedIn
  2. PlaywrightScraperService → scrapea RawText (Bronze)
  3. GeminiAnalyzerService → extrae Technologies, Frameworks, SeniorityLevel (Silver)
  4. Llama a JobTracker Pro API:
        POST /api/jobapplications
        {
          "userId": "...",
          "title": "Senior .NET Developer",
          "companyName": "WTW",
          "jobUrl": "https://linkedin.com/jobs/view/...",
          "description": "<RawText del scraping>",
          "source": "LinkedIn",
          "techStackBackend": "<Technologies de Gemini>",
          "techStackFrontend": "<Frameworks de Gemini>",
          "seniorityLevel": 4  ← Senior
        }
```

**El resultado:** abres JobTracker Pro y ves todas tus aplicaciones de LinkedIn
ya creadas, con el tech stack analizado por IA y el texto completo del anuncio.
Sin copy-paste manual.

---

## Referencia rapida de comandos Git para el flujo completo

```bash
# 1. Crear rama
git checkout -b feature/mi-feature

# 2. Trabajar (ciclo: editar → compilar → testear)
dotnet build
dotnet test

# 3. Commitear
git add archivo1.cs archivo2.tsx
git commit -m "feat: descripcion clara del cambio"

# 4. Subir rama
git push -u origin feature/mi-feature

# 5. Crear PR
gh pr create --title "..." --body "..."

# 6. Esperar que CI pase
gh pr checks   # muestra estado de los checks

# 7. Merge
gh pr merge --squash --delete-branch

# 8. Actualizar local
git checkout main
git pull origin main
git branch -d feature/mi-feature

# 9. Verificar que CI de main tambien pasa
gh run list --limit 3
```

---

_Guia creada el 2026-03-14. Para tu siguiente feature, sigue el mismo proceso desde el Paso 0._
