# JobTracker Pro — Full Technical Reference
## Complete Code Documentation, Concepts & Technology Guide

> NotebookLM source document — covers every layer of the application: Domain, Application, Infrastructure, API, Frontend, Testing, CI/CD, and Configuration.

---

## PART 0: TECHNOLOGY STACK & VERSIONS

### Backend (.NET)

| Project | Framework | Key Packages |
|---------|-----------|--------------|
| JobTrackerPro.Domain | net10.0 | (no dependencies) |
| JobTrackerPro.Application | net10.0 | MediatR 14.1.0, FluentValidation 12.1.1, BCrypt.Net-Next 4.1.0, AutoMapper 16.1.0 |
| JobTrackerPro.Infrastructure | net10.0 | EF Core 10.0.3, Npgsql.EFCore.PostgreSQL 10.0.0, StackExchange.Redis 2.11.8, Microsoft.Extensions.Caching.StackExchangeRedis 10.0.4, BCrypt.Net-Next 4.1.0, Microsoft.AspNetCore.Authentication.JwtBearer 10.0.4 |
| JobTrackerPro.Api | net10.0 (Web) | MediatR 14.1.0, Serilog.AspNetCore 10.0.0, Swashbuckle.AspNetCore 10.1.4, Microsoft.ApplicationInsights.AspNetCore 3.0.0, AutoMapper 16.1.0 |

### Frontend (Node / npm)

| Package | Version | Purpose |
|---------|---------|---------|
| react | ^19.2.0 | UI library |
| react-dom | ^19.2.0 | DOM rendering |
| react-router-dom | ^7.13.1 | Client-side routing |
| axios | ^1.13.6 | HTTP client |
| @tanstack/react-query | ^5.90.21 | Server state management (available, not fully wired) |
| tailwindcss | ^4.2.1 | Utility-first CSS framework |
| @tailwindcss/vite | ^4.2.1 | Vite plugin for Tailwind v4 |
| vite | ^7.3.1 | Build tool and dev server |
| typescript | ~5.9.3 | Type-safe JavaScript superset |
| @vitejs/plugin-react | ^5.1.1 | Vite plugin for JSX/React Fast Refresh |

### Test Projects

| Package | Version | Purpose |
|---------|---------|---------|
| xunit | (via SDK) | Test framework |
| Moq | (via SDK) | Mocking library |
| FluentAssertions | (via SDK) | Assertion library |
| Microsoft.AspNetCore.Mvc.Testing | (via SDK) | Integration test host |
| Microsoft.EntityFrameworkCore.InMemory | (via SDK) | In-memory DB for tests |

---

## PART 1: DOMAIN LAYER

### Concept: What is the Domain Layer?

The Domain layer is the innermost ring of Clean Architecture. It contains the core business concepts — entities, value objects, enums, and interfaces — with zero dependencies on any framework, database, or HTTP library. In C# projects, the Domain project has no `PackageReference` entries at all. Every other layer depends on Domain, but Domain depends on nothing.

### Concept: What is an Entity?

An entity (C# keyword: `class`) is an object that has a unique identity over time. Even if all its properties change, it is still the same object because it has the same `Id`. In this project, entities use the `Guid` type for their primary key and expose a private constructor plus a static `Create()` factory method to enforce valid state from birth.

### Concept: What is a Value Object?

A value object (C# keyword: `class` with private setters and no `Id`) is defined entirely by its properties. Two value objects with identical property values are considered equal. They are immutable: you cannot change a property after construction. In this project `TechStack` and `DateRange` are value objects. EF Core stores them as "owned entities" — their columns live in the same table as the owning entity rather than in a separate table.

### Concept: What is an Enum? (C# keyword: `enum`)

An `enum` is a named set of integer constants. The C# keyword is `enum`. Enums make code readable: instead of writing `status == 1` you write `status == ApplicationStatus.Applied`. The underlying integer is still what gets stored in the database. In this project three enums exist: `ApplicationStatus`, `SeniorityLevel`, and `WorkModality`.

### Concept: What is an Interface? (C# keyword: `interface`)

An `interface` defines a contract — a set of method and property signatures — without any implementation. Any class that declares it implements the interface must provide the implementation. The C# keyword is `interface`. In Clean Architecture, interfaces defined in the Domain layer allow higher layers to depend on abstractions rather than concrete classes, making the system testable and swappable.

---

### Class: JobApplication

**File:** `src/JobTrackerPro.Domain/Entities/JobApplication.cs`

`JobApplication` is the aggregate root of the system. It represents one job the user has tracked.

```csharp
public class JobApplication
{
    public Guid Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? JobUrl { get; private set; }
    public Guid CompanyId { get; private set; }
    public Company Company { get; private set; } = null!;
    public ApplicationStatus Status { get; private set; } = ApplicationStatus.Saved;
    public SeniorityLevel SeniorityLevel { get; private set; } = SeniorityLevel.NotSpecified;
    public WorkModality WorkModality { get; private set; } = WorkModality.NotSpecified;
    public TechStack TechStack { get; private set; } = TechStack.Create();
    public decimal? SalaryMin { get; private set; }
    public decimal? SalaryMax { get; private set; }
    public string? SalaryCurrency { get; private set; }
    public string? ContactName { get; private set; }
    public string? ContactEmail { get; private set; }
    public string? Source { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? AppliedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public string? Notes { get; private set; }
    public Guid UserId { get; private set; }

    private JobApplication() { }

    public static JobApplication Create(Guid userId, string title, Guid companyId,
        string? jobUrl = null, string? description = null, string? source = null) { ... }

    public void UpdateStatus(ApplicationStatus newStatus, string? notes = null) { ... }
    public void UpdateDetails(string title, string? jobUrl, string? notes) { ... }
    public void UpdateCompany(Guid companyId) { ... }
    public void SetTechStack(TechStack techStack) { ... }
    public void SetJobDetails(SeniorityLevel seniority, WorkModality modality) { ... }
    public void SetSalary(decimal? min, decimal? max, string? currency) { ... }
    public void SetContact(string? name, string? email) { ... }
    public void SetNotes(string? notes) { ... }
}
```

**Property-by-property explanation:**

- `Id` — `Guid`: A globally unique identifier. `Guid.NewGuid()` is called inside `Create()`. EF Core maps this to the primary key column.
- `Title` — `string`: The job title, e.g. "Senior .NET Developer". Required, max 200 characters.
- `Description` — `string?`: The nullable `?` means this field is optional. Raw text from the job posting.
- `JobUrl` — `string?`: Optional URL to the original job posting. Max 500 characters.
- `CompanyId` — `Guid`: Foreign key (FK) to the `Company` table. EF Core uses this to join the two tables.
- `Company` — `Company`: Navigation property. EF Core populates this automatically when you use `.Include(j => j.Company)` in a query. Marked `null!` (null-forgiving operator) because EF Core sets it after construction.
- `Status` — `ApplicationStatus`: The pipeline stage. Defaults to `Saved` (0).
- `SeniorityLevel` — `SeniorityLevel`: Defaults to `NotSpecified` (0).
- `WorkModality` — `WorkModality`: Defaults to `NotSpecified` (0).
- `TechStack` — `TechStack`: Value object with five string categories. Stored as owned entity in the same DB table with column-name prefix `TechStack_`.
- `SalaryMin`, `SalaryMax` — `decimal?`: Optional salary range. `decimal` is the .NET type for exact monetary values (avoids floating-point rounding).
- `SalaryCurrency` — `string?`: ISO currency code, e.g. "USD".
- `ContactName`, `ContactEmail` — `string?`: Optional recruiter contact information.
- `Source` — `string?`: Where the job was found ("LinkedIn", "Indeed", "Referral", etc.).
- `CreatedAt` — `DateTime`: Set once in `Create()` to `DateTime.UtcNow`. Never changes. UTC means Coordinated Universal Time — avoids timezone bugs.
- `AppliedAt` — `DateTime?`: Set automatically inside `UpdateStatus()` the first time status transitions to `Applied`. Null until then.
- `UpdatedAt` — `DateTime?`: Stamped by every behavior method. Used by the frontend stale detection logic.
- `Notes` — `string?`: Free-text notes written by the user.
- `UserId` — `Guid`: FK to the `User` who owns this application.

**Private constructor:** `private JobApplication() { }` — Required by EF Core, which uses reflection to reconstruct objects from the database. Making it private prevents accidental construction without required fields.

**Factory method `Create()`:**
- Validates that `title` is not null/whitespace; throws `ArgumentException` if it is.
- Trims whitespace from all string fields.
- Sets `Status = ApplicationStatus.Saved` and `CreatedAt = DateTime.UtcNow`.
- Returns a fully valid object.

**Behavior methods:**

- `UpdateStatus(newStatus, notes?)` — Sets `Status`, optionally sets `Notes`, stamps `UpdatedAt`. Critical side effect: if `newStatus == ApplicationStatus.Applied` and `AppliedAt` has never been set, sets `AppliedAt = DateTime.UtcNow`. This is domain logic — the rule lives here, not in a handler.
- `UpdateDetails(title, jobUrl, notes)` — Used by the PATCH endpoint. Updates title/URL/notes without touching status.
- `UpdateCompany(companyId)` — Reassigns the FK to a different company.
- `SetTechStack(techStack)` — Validates techStack is not null, then replaces the owned value object.
- `SetJobDetails(seniority, modality)` — Sets seniority level and work modality together.
- `SetSalary(min, max, currency)` — Validates min <= max before accepting values.
- `SetContact(name, email)` — Stores optional recruiter info.
- `SetNotes(notes)` — Trims and stores notes.

---

### Class: User

**File:** `src/JobTrackerPro.Domain/Entities/User.cs`

```csharp
public class User
{
    public Guid Id { get; private set; }
    public string FullName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public ICollection<JobApplication> JobApplications { get; private set; } = new List<JobApplication>();

    private User() { }

    public static User Create(string fullName, string email, string passwordHash) { ... }
    public void RecordLogin() { ... }
    public void ChangePassword(string newPasswordHash) { ... }
}
```

**Property explanations:**

- `FullName` — User's display name. Required, max 200 characters (enforced by `UserConfiguration`).
- `Email` — Unique login identifier. Stored as lowercase (`ToLowerInvariant()` in `Create()`). Has a unique index in the database.
- `PasswordHash` — The BCrypt-hashed password. **Never** the plain-text password. BCrypt is a one-way hash — you cannot reverse it.
- `LastLoginAt` — Optional; stamped by `RecordLogin()`. Currently called conceptually but not wired to `LoginHandler` explicitly.
- `JobApplications` — Navigation collection. EF Core populates this if you use `.Include(u => u.JobApplications)`.

**Behavior methods:**

- `Create(fullName, email, passwordHash)` — Validates fullName and email are non-empty. Normalizes email to lowercase. Does NOT hash the password — hashing happens in the Application layer before calling `Create()`.
- `RecordLogin()` — Sets `LastLoginAt = DateTime.UtcNow`.
- `ChangePassword(newPasswordHash)` — Validates hash is non-empty, replaces the stored hash.

---

### Class: Company

**File:** `src/JobTrackerPro.Domain/Entities/Company.cs`

```csharp
public class Company
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Website { get; private set; }
    public string? Industry { get; private set; }
    public string? Location { get; private set; }
    public string? Notes { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public ICollection<JobApplication> JobApplications { get; private set; } = new List<JobApplication>();

    private Company() { }

    public static Company Create(string name, string? website = null,
        string? industry = null, string? location = null) { ... }

    public void Update(string name, string? website, string? industry,
        string? location, string? notes) { ... }
}
```

Companies are created on-the-fly when a user adds an application. If a company with the same name already exists (case-insensitive lookup by `ICompanyRepository.GetByNameAsync`), the existing company record is reused. The `Name` column has a unique index.

**Key design decision:** Company records are shared across users. If two users track applications at "Google", both point to the same `Company` row. This avoids duplication.

---

### Class: RefreshToken

**File:** `src/JobTrackerPro.Domain/Entities/RefreshToken.cs`

```csharp
public class RefreshToken
{
    public Guid Id { get; private set; }
    public string Token { get; private set; } = string.Empty;
    public Guid UserId { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public bool IsRevoked { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public User? User { get; private set; }

    private RefreshToken() { }

    public static RefreshToken Create(Guid userId, int expirationDays = 7) => new() { ... };

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsActive => !IsRevoked && !IsExpired;

    public void Revoke() => IsRevoked = true;
}
```

**How token generation works:** `Token` is created by converting a new `Guid` to a byte array and then Base64-encoding it: `Convert.ToBase64String(Guid.NewGuid().ToByteArray())`. This produces a 24-character URL-safe string.

**Computed properties (expression-body members):**
- `IsExpired` — Returns true if the current UTC time is past `ExpiresAt`.
- `IsActive` — Returns true only if the token is neither revoked nor expired. Used by `RefreshTokenHandler` to validate incoming refresh requests.

**Token rotation:** On every use of a refresh token, the old one is revoked (`Revoke()`) and a brand-new `RefreshToken` is created. This prevents token replay attacks.

---

### Enum: ApplicationStatus

**File:** `src/JobTrackerPro.Domain/Enums/ApplicationStatus.cs`

```csharp
public enum ApplicationStatus
{
    Saved = 0,
    Applied = 1,
    Screening = 2,
    TechnicalTest = 3,
    Interview = 4,
    OfferReceived = 5,
    Accepted = 6,
    Rejected = 7,
    Withdrawn = 8
}
```

This is the **backend** enum. The values 0–8 are stored as integers in the PostgreSQL database. The frontend `ApplicationStatus` const object uses a **different** naming convention (see Part 5). The integer values 0–8 are identical between backend and frontend, but the names differ (e.g., backend `Saved=0` maps to frontend `Applied=0`). This mapping is intentional and must not be changed without updating both sides.

---

### Enum: SeniorityLevel

**File:** `src/JobTrackerPro.Domain/Enums/SeniorityLevel.cs`

```csharp
public enum SeniorityLevel
{
    NotSpecified = 0,
    Intern = 1,
    Junior = 2,
    Mid = 3,
    Senior = 4,
    Lead = 5,
    Staff = 6,
    Principal = 7
}
```

Attached to `JobApplication` to describe the seniority requirement of the position. Defaults to `NotSpecified`. Currently not exposed via the API's main CRUD endpoints but is stored in the database.

---

### Enum: WorkModality

**File:** `src/JobTrackerPro.Domain/Enums/WorkModality.cs`

```csharp
public enum WorkModality
{
    NotSpecified = 0,
    Remote = 1,
    Hybrid = 2,
    OnSite = 3
}
```

Indicates where the job is performed. Returned as a string in `JobApplicationDto` via `.ToString()` (e.g., "Remote", "Hybrid", "OnSite").

---

### Value Object: TechStack

**File:** `src/JobTrackerPro.Domain/ValueObjects/TechStack.cs`

```csharp
public class TechStack
{
    public string Backend { get; private set; } = string.Empty;
    public string Frontend { get; private set; } = string.Empty;
    public string Databases { get; private set; } = string.Empty;
    public string CloudAndDevOps { get; private set; } = string.Empty;
    public string Testing { get; private set; } = string.Empty;

    private TechStack() { }

    public static TechStack Create(string backend = "", string frontend = "",
        string databases = "", string cloudAndDevOps = "", string testing = "") { ... }

    public static TechStack FromFlatString(string technologies, string frameworks) { ... }
}
```

EF Core maps this as an **owned entity** using `OwnsOne()` in `JobApplicationConfiguration`. This means there is no separate `TechStack` table — the five columns (`TechStack_Backend`, `TechStack_Frontend`, etc.) live inside the `JobApplications` table row.

`FromFlatString()` is a legacy factory for compatibility with older data formats.

---

### Value Object: DateRange

**File:** `src/JobTrackerPro.Domain/ValueObjects/DateRange.cs`

```csharp
public class DateRange
{
    public DateTime Start { get; private set; }
    public DateTime? End { get; private set; }

    private DateRange() { }

    public static DateRange Create(DateTime start, DateTime? end = null) { ... }

    public int DaysElapsed => (int)((End ?? DateTime.UtcNow) - Start).TotalDays;
}
```

Validates that `End` is not before `Start`. `DaysElapsed` is a computed property: if no end date, it computes days from start to now. Currently defined but not actively used in the main CRUD flow.

---

### Interface: IJobApplicationRepository

**File:** `src/JobTrackerPro.Domain/Interfaces/IJobApplicationRepository.cs`

```csharp
public interface IJobApplicationRepository
{
    Task<JobApplication?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<JobApplication>> GetAllByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<JobApplication>> GetByStatusAsync(Guid userId, ApplicationStatus status, CancellationToken cancellationToken = default);
    Task AddAsync(JobApplication application, CancellationToken cancellationToken = default);
    Task UpdateAsync(JobApplication application, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    void Delete(JobApplication application);
}
```

- `Task<T>` means the method is asynchronous — the caller uses `await` to get the result without blocking the thread.
- `CancellationToken` allows the operation to be aborted if the HTTP request is cancelled (e.g., the user closes the browser).
- `IReadOnlyList<T>` means the returned collection cannot be modified by the caller.
- `Delete(JobApplication application)` is synchronous (no `Task`) because EF Core's `Remove()` is an in-memory operation; the actual DELETE SQL runs later when `SaveChangesAsync()` is called.

---

### Interface: IUserRepository

**File:** `src/JobTrackerPro.Domain/Interfaces/IUserRepository.cs`

```csharp
public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task AddAsync(User user, CancellationToken cancellationToken = default);
    Task UpdateAsync(User user, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string email, CancellationToken cancellationToken = default);
}
```

`ExistsAsync` is used by `RegisterHandler` to check for duplicate emails before creating a user.

---

### Interface: ICompanyRepository

**File:** `src/JobTrackerPro.Domain/Interfaces/ICompanyRepository.cs`

```csharp
public interface ICompanyRepository
{
    Task<Company?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Company?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Company>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Company company, CancellationToken cancellationToken = default);
    Task UpdateAsync(Company company, CancellationToken cancellationToken = default);
}
```

`GetByNameAsync` is the key method — used by `CreateJobApplicationHandler` and `EditJobApplicationHandler` to perform a case-insensitive lookup before deciding whether to create a new company record.

---

### Interface: IRefreshTokenRepository

**File:** `src/JobTrackerPro.Domain/Interfaces/IRefreshTokenRepository.cs`

```csharp
public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
}
```

No `UpdateAsync` is needed because `Revoke()` is called directly on the entity object and EF Core's change tracking detects the modification automatically.

---

### Interface: IUnitOfWork

**File:** `src/JobTrackerPro.Domain/Interfaces/IUnitOfWork.cs`

```csharp
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
```

The simplest interface in the system — one method. The implementation delegates directly to `ApplicationDbContext.SaveChangesAsync()`. Wrapping it in an interface allows handlers to be tested without a real database: a `Mock<IUnitOfWork>` can be injected.

---

## PART 2: APPLICATION LAYER

### Concept: What is CQRS?

CQRS stands for Command Query Responsibility Segregation. The idea is that operations that **change** state (Commands) are kept completely separate from operations that **read** state (Queries). Commands return a minimal result (like a `Guid` or `bool`). Queries return data (like a list of DTOs). This separation makes each operation focused and easier to reason about, test, and optimize independently.

### Concept: What is MediatR? What is ISender?

MediatR is a library that implements the Mediator design pattern in .NET. Instead of a controller calling a service directly, the controller sends a message (a command or query object) to MediatR, which finds the correct handler and calls it. This removes direct coupling between the API layer and the Application layer. `ISender` is the MediatR interface injected into controllers — it exposes the `Send()` method which dispatches a message to its handler.

### Concept: What is a Command vs a Query?

A **Command** is a message that changes state. Example: `CreateJobApplicationCommand`. A command typically returns either nothing, a `bool` (success/failure), or the ID of the newly created resource. A **Query** is a message that reads state without changing it. Example: `GetJobApplicationsQuery`. A query returns data. In this codebase, commands and queries implement `IRequest<TResponse>` from MediatR.

### Concept: What is a Pipeline Behavior?

A Pipeline Behavior (MediatR interface: `IPipelineBehavior<TRequest, TResponse>`) is middleware for the MediatR pipeline. When `sender.Send(command)` is called, MediatR runs all registered behaviors in order before calling the actual handler. This is how `ValidationBehavior` intercepts every command and runs FluentValidation validators before the handler executes. It's analogous to ASP.NET Core middleware but for the application layer.

### Concept: What is FluentValidation?

FluentValidation is a .NET library for writing strongly-typed validation rules using a fluent API. Instead of if-statements scattered through the code, you create a class that inherits `AbstractValidator<T>` and define rules using methods like `RuleFor(x => x.Title).NotEmpty().MaximumLength(200)`. Validators are auto-discovered from the assembly and injected by the DI container. When validation fails, a `ValidationException` is thrown which `ExceptionHandlingMiddleware` catches and returns as HTTP 400.

### Concept: What is a DTO (Data Transfer Object)?

A DTO is a simple data container with no business logic, used to transfer data between layers or across the network. In this project, `JobApplicationDto` is what the API returns to the frontend — it contains only the fields the client needs (no internal EF navigation properties, no private methods). DTOs are often C# `record` types, which are immutable value-type-style classes with auto-generated equality and constructor.

---

### Class: ValidationBehavior\<TRequest, TResponse\>

**File:** `src/JobTrackerPro.Application/Common/Behaviors/ValidationBehavior.cs`

```csharp
public class ValidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);

        var failures = _validators
            .Select(v => v.Validate(context))
            .SelectMany(result => result.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Count != 0)
            throw new ValidationException(failures);

        return await next();
    }
}
```

**How it works step by step:**
1. DI injects all `IValidator<TRequest>` instances registered for this command type.
2. If no validators exist for this command type, call `next()` immediately (pass through).
3. Create a `ValidationContext<TRequest>` wrapping the request object.
4. Run all validators and collect all `ValidationFailure` objects.
5. If any failures exist, throw `FluentValidation.ValidationException` — which `ExceptionHandlingMiddleware` will catch and convert to HTTP 400 with a `ValidationProblemDetails` body.
6. If validation passes, call `next()` to proceed to the actual handler.

The generic constraint `where TRequest : notnull` prevents nullable types from being used as commands.

---

### DTO: JobApplicationDto

**File:** `src/JobTrackerPro.Application/DTOs/JobApplicationDto.cs`

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
    string? Notes,
    DateTime CreatedAt,
    DateTime? AppliedAt,
    DateTime? UpdatedAt
);
```

C# `record` is a reference type that generates constructor, equality (`Equals`/`GetHashCode`), and `ToString` automatically from the declared properties.

**Critical design decisions:**
- `Status` is `int` — the integer value of the `ApplicationStatus` enum. It is NOT serialized as the string "Applied" or "Screening". This is intentional because the frontend filter compares `a.status === filter` where filter is a number.
- `CompanyName` is a flattened string from `a.Company?.Name`. The FK `CompanyId` is NOT included — the client only needs the name.
- `UpdatedAt` is included so the frontend can compute stale detection (7+ days no activity).
- `WorkModality` and `SeniorityLevel` are returned as strings (`.ToString()` on the enum), so "Remote", "Hybrid", etc.

---

### Command: CreateJobApplicationCommand + Validator + Handler

**File:** `src/JobTrackerPro.Application/JobApplications/Commands/CreateJobApplicationCommand.cs`

```csharp
public record CreateJobApplicationCommand(
    Guid UserId,
    string Title,
    string CompanyName,
    string? JobUrl = null,
    string? Description = null,
    string? Source = null
) : IRequest<Guid>;
```

`IRequest<Guid>` means this command, when handled, returns a `Guid` (the new application's ID).

**Validator:** `CreateJobApplicationValidator`

```csharp
public class CreateJobApplicationValidator : AbstractValidator<CreateJobApplicationCommand>
{
    public CreateJobApplicationValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("UserId is required.");
        RuleFor(x => x.Title).NotEmpty().WithMessage("Job title is required.")
            .MaximumLength(200).WithMessage("Job title must not exceed 200 characters.");
        RuleFor(x => x.CompanyName).NotEmpty().WithMessage("Company name is required.")
            .MaximumLength(100).WithMessage("Company name must not exceed 100 characters.");
        RuleFor(x => x.JobUrl)
            .MaximumLength(500).WithMessage("Job URL must not exceed 500 characters.")
            .Must(url => string.IsNullOrEmpty(url) || Uri.TryCreate(url, UriKind.Absolute, out _))
            .WithMessage("Job URL must be a valid URL.");
        RuleFor(x => x.Source).NotEmpty().WithMessage("Source is required.")
            .MaximumLength(50).WithMessage("Source must not exceed 50 characters.");
    }
}
```

The `Must()` rule uses `Uri.TryCreate()` to validate URL format. The `out _` discard pattern means we don't need the parsed URI object — we only care whether parsing succeeds.

**Handler:** `CreateJobApplicationHandler`

Logic flow:
1. Log that creation is starting.
2. Look up the company by name via `ICompanyRepository.GetByNameAsync()`.
3. If company does not exist, create it with `Company.Create()` and call `AddAsync()`.
4. Create the application with `JobApplication.Create()`.
5. Add the application via `IJobApplicationRepository.AddAsync()`.
6. Commit with `IUnitOfWork.SaveChangesAsync()`.
7. Invalidate the Redis cache key `job-applications:{userId}` so the next GET fetches fresh data.
8. Return the new application's `Id`.

---

### Command: UpdateJobApplicationCommand + Validator + Handler

**File:** `src/JobTrackerPro.Application/JobApplications/Commands/UpdateJobApplicationCommand.cs`

```csharp
public record UpdateJobApplicationCommand(
    Guid Id,
    ApplicationStatus NewStatus,
    string? Notes
) : IRequest<bool>;
```

Maps to `PUT /api/jobapplications/{id}`. Returns `true` if found and updated, `false` if not found (which becomes HTTP 404).

**Validator:** `UpdateJobApplicationValidator`

```csharp
RuleFor(x => x.Id).NotEmpty().WithMessage("Application Id is required.");
RuleFor(x => x.NewStatus).IsInEnum().WithMessage("Invalid application status value.");
RuleFor(x => x.Notes).MaximumLength(1000).WithMessage("Notes must not exceed 1000 characters.");
```

`IsInEnum()` validates that the integer value falls within the defined `ApplicationStatus` enum range.

**Handler:** `UpdateJobApplicationHandler`

1. Fetch application by ID.
2. If null, return `false`.
3. Call `application.UpdateStatus(request.NewStatus, request.Notes)` — domain method that also sets `AppliedAt` if applicable.
4. Save via `IUnitOfWork.SaveChangesAsync()`.
5. Return `true`.

---

### Command: EditJobApplicationCommand + Handler

**File:** `src/JobTrackerPro.Application/JobApplications/Commands/EditJobApplicationCommand.cs`

```csharp
public record EditJobApplicationCommand(
    Guid Id,
    string Title,
    string? CompanyName,
    string? JobUrl,
    string? Notes
) : IRequest<bool>;
```

Maps to `PATCH /api/jobapplications/{id}`. Edits metadata (title, company, URL, notes) without touching the status pipeline.

**Handler:** `EditJobApplicationHandler`

1. Fetch application by ID; return `false` if not found.
2. Call `application.UpdateDetails(title, jobUrl, notes)`.
3. If `CompanyName` is provided, look up or create the company and call `application.UpdateCompany(company.Id)`.
4. Save and return `true`.

---

### Command: DeleteJobApplicationCommand + Handler

```csharp
public record DeleteJobApplicationCommand(Guid Id) : IRequest<bool>;
```

**Handler:** `DeleteJobApplicationHandler`

1. Fetch application by ID; return `false` if not found.
2. Call `_repository.Delete(application)` — the synchronous method that marks the entity for deletion in EF Core's change tracker.
3. Save with `_unitOfWork.SaveChangesAsync()`.
4. Return `true`.

---

### Query: GetJobApplicationsQuery + Handler

```csharp
public record GetJobApplicationsQuery(Guid UserId)
    : IRequest<IReadOnlyList<JobApplicationDto>>;
```

**Handler:** `GetJobApplicationsHandler`

Cache-first read strategy:
1. Build cache key: `job-applications:{userId}`.
2. Call `_cache.GetAsync<List<JobApplicationDto>>(cacheKey)`.
3. If a non-null result is returned, return it immediately (cache hit, no DB query).
4. On cache miss, call `_repository.GetAllByUserIdAsync(userId)`.
5. Map each `JobApplication` entity to a `JobApplicationDto` using an inline LINQ projection (no AutoMapper used here).
6. Store the list in Redis with a 10-minute TTL: `_cache.SetAsync(cacheKey, dtos, TimeSpan.FromMinutes(10))`.
7. Return the list.

**Status integer conversion:** `Status: (int)a.Status` — explicit cast from the `ApplicationStatus` enum to its underlying `int` value.

---

### Auth Commands: RegisterCommand, LoginCommand, RefreshTokenCommand

**RegisterCommand**

```csharp
public record RegisterCommand(string FullName, string Email, string Password)
    : IRequest<AuthResponse>;
```

**RegisterHandler logic:**
1. Check if email already exists via `IUserRepository.ExistsAsync()`. If yes, throw `InvalidOperationException` (becomes HTTP 500 via global handler).
2. Hash the password: `BCrypt.Net.BCrypt.HashPassword(request.Password)`.
3. Create the user: `User.Create(fullName, email, passwordHash)`.
4. Create a `RefreshToken` and add it.
5. Save everything in one `SaveChangesAsync()` call.
6. Generate an access token: `_jwtTokenGenerator.GenerateToken(user)`.
7. Return `AuthResponse(accessToken, refreshToken.Token, expiresAt)`.

**LoginCommand**

```csharp
public record LoginCommand(string Email, string Password) : IRequest<AuthResponse>;
```

**LoginHandler logic:**
1. Look up user by email.
2. If null OR if `BCrypt.Net.BCrypt.Verify(password, user.PasswordHash)` is false, throw `UnauthorizedAccessException`.
3. Create a new `RefreshToken`, save, generate access token.
4. Return `AuthResponse`.

**RefreshTokenCommand**

```csharp
public record RefreshTokenCommand(string Token) : IRequest<AuthResponse>;
```

**RefreshTokenHandler logic (token rotation):**
1. Look up the `RefreshToken` entity by the token string (includes the `User` navigation property via `.Include(r => r.User)`).
2. If not found or `!refreshToken.IsActive`, throw `UnauthorizedAccessException`.
3. Call `refreshToken.Revoke()` — marks old token as revoked.
4. Create a new `RefreshToken` with `RefreshToken.Create(refreshToken.UserId)`.
5. Save both (old revoked + new).
6. Generate new access token from `refreshToken.User!` (navigation property loaded by Include).
7. Return new `AuthResponse`.

**AuthResponse DTO:**

```csharp
public record AuthResponse(string AccessToken, string RefreshToken, DateTime ExpiresAt);
```

---

### Command: ChangePasswordCommand + Handler

```csharp
public record ChangePasswordCommand(
    Guid UserId,
    string CurrentPassword,
    string NewPassword
) : IRequest;
```

`IRequest` (no generic argument) means this command returns nothing — equivalent to `void`.

**Handler logic:**
1. Fetch user by `UserId`. If null, throw `KeyNotFoundException`.
2. Verify current password: `BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash)`. If fails, throw `UnauthorizedAccessException`.
3. Hash the new password: `BCrypt.Net.BCrypt.HashPassword(request.NewPassword)`.
4. Call `user.ChangePassword(newHash)` — domain method.
5. Save.

---

## PART 3: INFRASTRUCTURE LAYER

### Concept: What is Entity Framework Core?

Entity Framework Core (EF Core) is Microsoft's Object-Relational Mapper (ORM) for .NET. An ORM maps C# classes to database tables automatically. You write C# LINQ queries like `_context.JobApplications.Where(j => j.UserId == userId).ToListAsync()` and EF Core translates them to SQL. You never write SQL by hand in the main application code.

### Concept: What is a DbContext?

A `DbContext` (C# class inheriting `Microsoft.EntityFrameworkCore.DbContext`) is the central EF Core class. It represents a session with the database. It holds `DbSet<T>` properties (one per entity), tracks changes to entities in memory, and writes those changes to the DB when you call `SaveChangesAsync()`. In this project the class is `ApplicationDbContext`.

### Concept: What is a Migration?

An EF Core migration is a C# file that describes a schema change (create table, add column, etc.). When your entity classes change, you run `dotnet ef migrations add <Name>` to generate the migration file. On startup, `db.Database.Migrate()` applies all pending migrations to the actual database. This project guards that call with `!app.Environment.IsEnvironment("Testing")` so integration tests (which use an InMemory database) do not try to run migrations.

### Concept: What is the Repository Pattern?

The Repository pattern wraps data access behind an interface. Instead of controllers calling EF Core directly, they call `IJobApplicationRepository.GetByIdAsync()`. The concrete EF Core implementation (`JobApplicationRepository`) lives in Infrastructure. This means the Application and Domain layers never reference EF Core — making them testable with mocks and swappable to a different database.

### Concept: What is the Unit of Work Pattern?

The Unit of Work pattern groups multiple repository operations into a single database transaction. In this project, `IUnitOfWork.SaveChangesAsync()` is called exactly once per handler, after all repository `AddAsync`/`UpdateAsync`/`Delete` calls have been made. This ensures atomicity: either all changes commit together or none do.

### Concept: What is Redis? What is a Cache?

Redis is an in-memory key-value store used as a distributed cache. A cache stores the results of expensive operations (like database queries) in fast memory so subsequent requests can skip the DB entirely. In this project, `GetJobApplicationsHandler` stores the user's application list in Redis for 10 minutes. The key format is `job-applications:{userId}`. When a mutation (create, delete, update status) occurs, `CreateJobApplicationHandler` invalidates the cache by removing the key, so the next GET fetches fresh data. If Redis is unavailable, `RedisCacheService` catches the exception, logs a warning, and falls back gracefully to the database path.

### Concept: What is JWT (JSON Web Token)?

A JWT (JSON Web Token) is a compact, signed token used for stateless authentication. It has three Base64-encoded parts separated by dots: `header.payload.signature`. The payload contains **claims** — key-value pairs like `sub` (subject/user ID), `email`, `given_name`, and `jti` (unique token ID). The server signs the token with a secret key using HMAC-SHA256 (`HS256`). The client sends the JWT in the `Authorization: Bearer <token>` header on every request. The server validates the signature without querying the database, making it stateless. Access tokens expire (60 minutes in this project); refresh tokens extend the session.

### Concept: What is BCrypt?

BCrypt is a password hashing algorithm designed to be computationally slow, making brute-force attacks impractical. `BCrypt.Net.BCrypt.HashPassword(password)` produces a hash that includes a salt (random data) so two identical passwords produce different hashes. `BCrypt.Verify(inputPassword, storedHash)` checks whether a plain-text password matches a stored hash. Passwords are **never** stored in plain text anywhere in this project.

### Concept: What is a BackgroundService?

A `BackgroundService` (C# base class from `Microsoft.Extensions.Hosting`) is a long-running background process that runs for the lifetime of the application. You override the `ExecuteAsync(CancellationToken)` method with a loop. In this project, `StaleNotificationService` runs once every 24 hours to email users about stale applications. Background services are registered with `services.AddHostedService<T>()`.

---

### Class: ApplicationDbContext

**File:** `src/JobTrackerPro.Infrastructure/Data/ApplicationDbContext.cs`

```csharp
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<JobApplication> JobApplications => Set<JobApplication>();
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
```

- `DbContextOptions<ApplicationDbContext>` is injected by the DI container (configured in `DependencyInjection.cs` to use Npgsql/PostgreSQL in production, InMemory in tests).
- Each `DbSet<T>` maps to a database table. The expression-body `=> Set<T>()` is equivalent to a read-only property backed by EF Core's internal set registry.
- `OnModelCreating` is where EF Core schema configuration happens. `ApplyConfigurationsFromAssembly()` scans the Infrastructure assembly and applies all `IEntityTypeConfiguration<T>` classes (the three configuration files in `Configurations/`).

**Entity Configurations:**

`JobApplicationConfiguration`:
- Primary key: `Id`.
- `Title`: required, max 200.
- `JobUrl`: max 500; `Source`: max 100; `SalaryCurrency`: max 10.
- `TechStack` owned entity: five columns with `TechStack_` prefix, each max 500.
- Relationship: many-to-one with `Company` via FK `CompanyId`.

`UserConfiguration`:
- Primary key: `Id`.
- `FullName`: required, max 200.
- `Email`: required, max 200. **Unique index** — prevents duplicate email registration at the DB level.
- `PasswordHash`: required (no max length — BCrypt hashes are fixed-length strings).

`CompanyConfiguration`:
- Primary key: `Id`.
- `Name`: required, max 200. **Unique index** — one row per company name.
- `Website`: max 300; `Industry`: max 100; `Location`: max 200.

---

### Class: JobApplicationRepository

**File:** `src/JobTrackerPro.Infrastructure/Persistence/Repositories/JobApplicationRepository.cs`

EF Core implementation of `IJobApplicationRepository`. Receives `ApplicationDbContext` via constructor injection (dependency injection).

Key implementation details:
- `GetByIdAsync` and `GetAllByUserIdAsync` use `.Include(j => j.Company)` — this tells EF Core to also load the related `Company` record in the same SQL query (JOIN), so `j.Company.Name` is available without a second round trip.
- `GetAllByUserIdAsync` orders results by `CreatedAt` descending — newest applications appear first.
- `UpdateAsync` calls `_context.JobApplications.Update(application)` — this sets the entity's state to `Modified`, so all columns are included in the UPDATE SQL even if only one changed.
- `Delete(JobApplication application)` — synchronous, calls `_context.JobApplications.Remove(application)` which marks the entity `Deleted` in the change tracker. The DELETE SQL runs on the next `SaveChangesAsync()`.

---

### Class: UserRepository

**File:** `src/JobTrackerPro.Infrastructure/Persistence/Repositories/UserRepository.cs`

- `GetByIdAsync` uses `FindAsync([id])` — EF Core's optimized primary-key lookup that checks the change-tracker cache before querying the DB.
- `GetByEmailAsync` normalizes input to lowercase before querying (`.ToLowerInvariant()`), matching how `User.Create()` stores emails.
- `ExistsAsync` uses `AnyAsync()` — generates a SQL `EXISTS` query, which is more efficient than fetching the full row just to check existence.

---

### Class: CompanyRepository

**File:** `src/JobTrackerPro.Infrastructure/Persistence/Repositories/CompanyRepository.cs`

- `GetByNameAsync` compares `c.Name.ToLower() == name.ToLower()` — case-insensitive name matching.
- `GetAllAsync` orders by `Name` alphabetically.

---

### Class: JwtTokenGenerator

**File:** `src/JobTrackerPro.Infrastructure/Authentication/JwtTokenGenerator.cs`

```csharp
public string GenerateToken(User user)
{
    var signingCredentials = new SigningCredentials(
        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret)),
        SecurityAlgorithms.HmacSha256);

    var claims = new[]
    {
        new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
        new Claim(JwtRegisteredClaimNames.Email, user.Email),
        new Claim(JwtRegisteredClaimNames.GivenName, user.FullName),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

    var token = new JwtSecurityToken(
        issuer: _jwtSettings.Issuer,
        audience: _jwtSettings.Audience,
        claims: claims,
        expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
        signingCredentials: signingCredentials);

    return new JwtSecurityTokenHandler().WriteToken(token);
}
```

- `SymmetricSecurityKey` — the signing secret is converted from UTF-8 string to bytes. Must match the key used for validation in `DependencyInjection.cs`.
- `HmacSha256` — HMAC (Hash-based Message Authentication Code) with SHA-256. Both signing and verification use the same secret (symmetric).
- Claims: `sub` = user GUID (the frontend decodes this to get the `userId`), `email`, `given_name`, `jti` = unique token ID (prevents token replay if a revocation list is maintained).
- `_jwtSettings.ExpirationMinutes` defaults to 60.

**JwtSettings class:**
```csharp
public class JwtSettings
{
    public string Secret { get; init; } = string.Empty;
    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public int ExpirationMinutes { get; init; } = 60;
}
```

`init` properties can only be set during object initialization — they are read-only after construction, which is appropriate for configuration objects.

---

### Class: RedisCacheService

**File:** `src/JobTrackerPro.Infrastructure/Caching/RedisCacheService.cs`

Implements `ICacheService` using `IDistributedCache` (the ASP.NET Core distributed cache abstraction), which is backed by `StackExchange.Redis`.

**GetAsync\<T\>:**
1. Call `_cache.GetStringAsync(key)` to fetch the JSON string.
2. If null (cache miss), return `default` (null for reference types).
3. If found, deserialize with `JsonSerializer.Deserialize<T>(data)`.
4. Wrapped in try/catch — if Redis is unreachable, log a warning and return `default` (graceful fallback to DB).

**SetAsync\<T\>:**
1. Create `DistributedCacheEntryOptions` with `AbsoluteExpirationRelativeToNow` set to the provided TTL (or 10 minutes default).
2. Serialize the value to JSON.
3. Call `_cache.SetStringAsync()`.
4. Wrapped in try/catch — if Redis is unreachable, log warning and continue.

**RemoveAsync:**
1. Call `_cache.RemoveAsync(key)`.
2. Wrapped in try/catch — failure to invalidate cache is logged but does not throw.

The try/catch blocks are the fallback mechanism. Do not remove them.

---

### Class: SmtpEmailService + IEmailService interface

**IEmailService:**
```csharp
public interface IEmailService
{
    Task SendAsync(string to, string subject, string htmlBody,
        CancellationToken cancellationToken = default);
}
```

**SmtpEmailService:**
- Reads `EmailSettings` from DI (configured from `appsettings.json`).
- If `_settings.Enabled == false`, logs the email content at Information level and returns without sending. This is the "development mode" — no actual SMTP needed.
- If enabled, creates an `SmtpClient` with SSL, sends the HTML email.

**EmailSettings defaults:**
- `Enabled`: false (emails are off by default)
- `SmtpHost`: "smtp.gmail.com"
- `SmtpPort`: 587
- `FromEmail`: "noreply@jobtrackerpro.dev"
- `FromName`: "JobTracker Pro"

---

### Class: StaleNotificationService

**File:** `src/JobTrackerPro.Infrastructure/BackgroundServices/StaleNotificationService.cs`

Inherits `BackgroundService`. Runs a loop every 24 hours.

**Active statuses monitored:**
```csharp
private static readonly ApplicationStatus[] ActiveStatuses = [
    ApplicationStatus.Saved,
    ApplicationStatus.Applied,
    ApplicationStatus.Screening,
    ApplicationStatus.TechnicalTest,
    ApplicationStatus.Interview,
];
```

**CheckAndNotifyAsync logic:**
1. Create a DI scope (because `ApplicationDbContext` is scoped, not singleton).
2. Compute cutoff date: `DateTime.UtcNow.AddDays(-7)`.
3. Query `JobApplications` where status is in `ActiveStatuses` AND `(UpdatedAt ?? CreatedAt) < cutoff`.
4. Load related companies with `.Include(a => a.Company)`.
5. Fetch the users for those application UserIds.
6. Group applications by user, build an HTML email body listing the stale applications.
7. Send via `IEmailService.SendAsync()`.
8. Log summary.

**DI scoping note:** `IServiceScopeFactory` is injected (singleton-safe) rather than `ApplicationDbContext` directly. A new scope is created per execution to properly resolve scoped services from a singleton background service.

---

### Class: DependencyInjection (Infrastructure)

**File:** `src/JobTrackerPro.Infrastructure/DependencyInjection.cs`

Extension method `AddInfrastructure(IServiceCollection, IConfiguration)` registers:

1. **EF Core + PostgreSQL:** `services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString))`
2. **Repositories** (all scoped): `IJobApplicationRepository` → `JobApplicationRepository`, `ICompanyRepository` → `CompanyRepository`, `IUserRepository` → `UserRepository`, `IRefreshTokenRepository` → `RefreshTokenRepository`
3. **Unit of Work** (scoped): `IUnitOfWork` → `UnitOfWork`
4. **JWT Settings:** `services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"))`
5. **JWT Token Generator** (scoped): `IJwtTokenGenerator` → `JwtTokenGenerator`
6. **JWT Authentication:** `services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(...)` — configures token validation with issuer, audience, lifetime, and signing key checks.
7. **Redis:** `services.AddStackExchangeRedisCache(options => { options.Configuration = redisConnection; options.InstanceName = "JobTrackerPro:"; })` — falls back to "localhost:6379" if no connection string configured.
8. **Cache service** (scoped): `ICacheService` → `RedisCacheService`
9. **Email settings + service:** `IEmailService` → `SmtpEmailService`
10. **Background service:** `services.AddHostedService<StaleNotificationService>()`

Scoped lifetime means one instance per HTTP request. Singleton means one instance for the entire application lifetime.

---

## PART 4: API LAYER

### Concept: What is ASP.NET Core?

ASP.NET Core is Microsoft's cross-platform web framework. It handles HTTP request/response processing, routing, middleware, authentication, and JSON serialization. Controllers in ASP.NET Core receive HTTP requests, call Application layer handlers via MediatR, and return HTTP responses. The framework is configured in `Program.cs`.

### Concept: What is a Controller?

A Controller (C# class inheriting `ControllerBase`, decorated with `[ApiController]` and `[Route]`) is an ASP.NET Core class that maps HTTP routes to C# methods. `[ApiController]` enables automatic model binding from JSON and automatic 400 responses for model validation errors. `[Route("api/[controller]")]` sets the base route — `[controller]` is replaced with the class name minus "Controller".

### Concept: What is Middleware?

Middleware in ASP.NET Core is a function in the HTTP request pipeline. Each middleware can inspect/modify the request, call `next()` to pass control to the next middleware, and then inspect/modify the response on the way back. The order of `app.Use*()` calls in `Program.cs` determines execution order. `ExceptionHandlingMiddleware` sits early in the pipeline so it catches exceptions from all subsequent middleware and controllers.

### Concept: What is CORS?

CORS (Cross-Origin Resource Sharing) is a browser security mechanism. By default, browsers block JavaScript from calling APIs on a different origin (domain/port/protocol). The server must explicitly allow origins via CORS headers. In this project, the policy named "Frontend" allows `http://localhost:5173`, `http://localhost:5174`, and `https://ramiro671.github.io`. Other origins are rejected by the browser (not by the server).

### Concept: What is Rate Limiting?

Rate limiting restricts how many requests a client can make within a time window, protecting against brute-force attacks and abuse. This project uses ASP.NET Core's built-in `AddRateLimiter` with two fixed-window policies:
- "api" — 60 requests per minute per IP for general endpoints.
- "auth" — 10 requests per minute per IP for login/register endpoints (brute-force protection).
Rate limiting is disabled in the Testing environment because all integration test requests share one IP address.

### Concept: What is Swagger/OpenAPI?

Swagger (via `Swashbuckle.AspNetCore`) auto-generates an interactive API documentation UI from your controllers and XML comments. It is served at the root URL (`/`) in this project (`RoutePrefix = string.Empty`). It also allows you to test endpoints directly in the browser and includes Bearer token support for authenticated endpoints.

---

### Class: Program.cs (middleware pipeline)

**File:** `src/JobTrackerPro.Api/Program.cs`

The top-level statements in `Program.cs` follow the ASP.NET Core minimal hosting model.

**Bootstrap logger:** Serilog is initialized before the app builder with `Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger()` — this catches startup errors.

**Service registration (builder phase):**
1. `builder.Host.UseSerilog(...)` — replaces default Microsoft logging with Serilog. Outputs to console (with structured template) and to rolling daily files at `logs/jobtracker-.log`.
2. `builder.Services.AddCors(...)` — registers the "Frontend" CORS policy.
3. `builder.Services.AddRateLimiter(...)` — registers rate limiters (only if NOT in Testing environment).
4. `builder.Services.AddApplication()` — MediatR + FluentValidation (from Application layer DI).
5. `builder.Services.AddInfrastructure(builder.Configuration)` — EF Core, repositories, JWT, Redis, email, background service (from Infrastructure layer DI).
6. Application Insights telemetry (conditional on connection string being present).
7. `builder.Services.AddAuthorization()` — enables the `[Authorize]` attribute.
8. `builder.Services.AddControllers()` — enables controller routing.
9. `builder.Services.AddSwaggerGen(...)` — configures Swagger UI with Bearer auth support.

**App pipeline (middleware order):**
1. `app.Database.Migrate()` — applies pending EF migrations (skipped in Testing).
2. `app.UseSerilogRequestLogging(...)` — logs each HTTP request with method, path, status code, and duration.
3. `app.UseCors("Frontend")` — applies CORS headers. Must come before `UseAuthentication`.
4. `app.UseRateLimiter()` — (only if not Testing).
5. `app.UseMiddleware<ExceptionHandlingMiddleware>()` — global error handler.
6. `app.UseSwagger()` + `app.UseSwaggerUI(...)` — serves OpenAPI JSON and Swagger UI at root.
7. `app.UseHttpsRedirection()` — redirects HTTP to HTTPS.
8. `app.UseAuthentication()` — validates JWT tokens.
9. `app.UseAuthorization()` — checks `[Authorize]` attributes.
10. `app.MapControllers()` — wires routes to controllers.
11. `app.Run()` — starts the HTTP server.

**`public partial class Program { }`** — This empty partial class declaration at the bottom makes the `Program` class accessible to the integration test project via `WebApplicationFactory<Program>`.

---

### Class: ExceptionHandlingMiddleware

**File:** `src/JobTrackerPro.Api/Middleware/ExceptionHandlingMiddleware.cs`

Catches all unhandled exceptions and returns RFC 9110 ProblemDetails JSON responses. The `InvokeAsync` method wraps `await _next(context)` in try/catch blocks:

| Exception Type | HTTP Status | Response Type |
|---------------|-------------|---------------|
| `FluentValidation.ValidationException` | 400 Bad Request | `ValidationProblemDetails` with field-level errors |
| `UnauthorizedAccessException` | 401 Unauthorized | `ProblemDetails` with exception message |
| Any other `Exception` | 500 Internal Server Error | `ProblemDetails` with generic message (detail hidden from client) |

`ValidationProblemDetails` groups errors by property name:
```json
{
  "title": "Validation failed",
  "status": 400,
  "errors": {
    "Title": ["Job title is required.", "Job title must not exceed 200 characters."],
    "CompanyName": ["Company name is required."]
  }
}
```

---

### Controller: JobApplicationsController

**File:** `src/JobTrackerPro.Api/Controllers/JobApplicationsController.cs`

```
[Route("api/[controller]")]  →  base path: /api/jobapplications
[Authorize]                  →  all endpoints require valid JWT
[EnableRateLimiting("api")]  →  60 req/min per IP
```

Injects `ISender` (MediatR) — the only dependency. All business logic is delegated to MediatR handlers.

**Endpoint 1: GET /api/jobapplications/{userId}**
- Method: `HttpGet("{userId:guid}")`
- Request: userId in URL path. Authorization header required.
- Response: 200 OK with `List<JobApplicationDto>` JSON array
- Sample response:
```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "title": "Senior .NET Developer",
    "companyName": "Anthropic",
    "status": 1,
    "workModality": "Remote",
    "seniorityLevel": "Senior",
    "jobUrl": "https://anthropic.com/jobs/1",
    "source": "LinkedIn",
    "salaryMin": null,
    "salaryMax": null,
    "salaryCurrency": null,
    "notes": "Applied via referral",
    "createdAt": "2026-03-01T10:00:00Z",
    "appliedAt": "2026-03-02T09:00:00Z",
    "updatedAt": "2026-03-02T09:00:00Z"
  }
]
```

**Endpoint 2: POST /api/jobapplications**
- Method: `HttpPost`
- Request body (JSON):
```json
{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "title": "Senior .NET Developer",
  "companyName": "Anthropic",
  "jobUrl": "https://anthropic.com/jobs/1",
  "description": "Remote position",
  "source": "LinkedIn"
}
```
- Response: 201 Created with `{ "id": "<guid>" }` and `Location` header pointing to GET endpoint.
- Error: 400 Bad Request if title/companyName/source empty or jobUrl invalid.

**Endpoint 3: PUT /api/jobapplications/{id}**
- Method: `HttpPut("{id:guid}")`
- Request body (JSON):
```json
{ "newStatus": 1, "notes": "Applied online today" }
```
- The controller wraps this in `UpdateJobApplicationCommand(id, request.NewStatus, request.Notes)`.
- Response: 204 No Content on success; 404 Not Found if ID not found.

**Endpoint 4: PATCH /api/jobapplications/{id}**
- Method: `HttpPatch("{id:guid}")`
- Request body (JSON):
```json
{ "title": "New Title", "companyName": "NewCo", "jobUrl": "https://...", "notes": "Updated" }
```
- Uses `command with { Id = id }` — C# record `with` expression creates a copy of the command with `Id` overridden (the body comes from `[FromBody]` but the ID comes from the URL).
- Response: 204 No Content or 404 Not Found.

**Endpoint 5: DELETE /api/jobapplications/{id}**
- Method: `HttpDelete("{id:guid}")`
- Request: ID in URL path, no body.
- Response: 204 No Content or 404 Not Found.

---

### Controller: AuthController

**File:** `src/JobTrackerPro.Api/Controllers/AuthController.cs`

```
[Route("api/[controller]")]  →  base path: /api/auth
[EnableRateLimiting("auth")] →  10 req/min per IP (no [Authorize])
```

**POST /api/auth/register**
- Request body: `{ "fullName": "string", "email": "string", "password": "string" }`
- Response: 200 OK with `AuthResponse`:
```json
{
  "accessToken": "eyJhbGci...",
  "refreshToken": "abc123==",
  "expiresAt": "2026-03-15T11:00:00Z"
}
```
- Error: 500 Internal Server Error if email already registered (thrown as `InvalidOperationException`).

**POST /api/auth/login**
- Request body: `{ "email": "string", "password": "string" }`
- Response: 200 OK with `AuthResponse`
- Error: 401 Unauthorized if credentials invalid.

**POST /api/auth/refresh**
- Request body: `{ "token": "abc123==" }` (the refresh token string)
- Response: 200 OK with new `AuthResponse` (rotated tokens)
- Error: 401 Unauthorized if refresh token is expired, revoked, or not found.

---

### Controller: UsersController

**File:** `src/JobTrackerPro.Api/Controllers/UsersController.cs`

```
[Route("api/[controller]")]  →  base path: /api/users
[Authorize]
[EnableRateLimiting("api")]
```

**PUT /api/users/me/password**
- Request body: `{ "currentPassword": "string", "newPassword": "string" }`
- Extracts `userId` from the JWT claim `ClaimTypes.NameIdentifier` (or `"sub"` as fallback). Returns 401 if the claim is missing or not a valid GUID.
- Response: 204 No Content on success.
- Errors: 401 if current password wrong; 404 if user not found.

---

## PART 5: FRONTEND

### Concept: What is React?

React is a JavaScript/TypeScript library for building user interfaces. The core idea is the **component** — a function that takes props (inputs) and returns JSX (HTML-like syntax). React maintains a virtual DOM and efficiently updates only the changed parts of the real DOM. Components can have local state (`useState` hook) and respond to lifecycle events (`useEffect` hook).

### Concept: What is TypeScript?

TypeScript is a superset of JavaScript that adds static type annotations. Instead of `let x = 5` you can write `let x: number = 5`. TypeScript catches type errors at compile time (before the code runs), making large codebases much safer to refactor. All `.tsx` files in this project are TypeScript + JSX (React components).

### Concept: What is Vite?

Vite is a modern build tool for frontend projects. It serves files in development using native ES modules (extremely fast — no bundling during dev), and bundles for production using Rollup. In this project, `vite.config.ts` uses the React plugin (for JSX/Fast Refresh) and the Tailwind CSS v4 Vite plugin. The `base: '/jobtracker-pro/'` setting makes all asset URLs relative to `/jobtracker-pro/` — required because the app is hosted at `https://ramiro671.github.io/jobtracker-pro/`.

### Concept: What is Tailwind CSS?

Tailwind CSS is a utility-first CSS framework. Instead of writing custom CSS classes, you compose small utility classes directly in HTML/JSX: `className="bg-white rounded-xl border border-gray-200 p-5"`. Version 4 (used here) installs as a Vite plugin and processes CSS with `@import "tailwindcss"` in `index.css`. Dark mode is enabled via a class strategy: adding `dark` class to `<html>` activates dark variant styles.

### Concept: What is a React Context?

A React Context provides a way to share state across an entire component tree without passing props through every level ("prop drilling"). The `createContext()` function creates the context. A `Provider` component wraps the tree and exposes a value. Any descendant component can call `useContext()` to read that value. Custom hooks like `useAuth()`, `useTheme()`, and `useToast()` wrap `useContext()` with a safety check.

### Concept: What is a React Hook?

A Hook is a function that lets React components "hook into" React features. The built-in hooks used in this project:
- `useState<T>(initial)` — declares a state variable and a setter function.
- `useEffect(fn, deps)` — runs a side effect when `deps` changes. Empty `[]` means "run once on mount."
- `useMemo(fn, deps)` — memoizes a computed value; recomputes only when `deps` change (used conceptually for filtered lists via inline `filter()` calls).
- `useCallback(fn, deps)` — memoizes a function reference (used in `ToastContext`).
- `useContext(Context)` — reads a context value.
- `useNavigate()`, `useAuth()`, `useToast()`, `useTheme()` — custom hooks wrapping context and routing.

### Concept: What is Axios?

Axios is a Promise-based HTTP client for JavaScript/TypeScript. It wraps the browser's `fetch` API with convenient features: automatic JSON serialization/deserialization, interceptors (middleware for requests/responses), and error handling. In this project, a single `apiClient` instance is created with `axios.create({ baseURL, headers })`. Request and response interceptors are added to auto-attach JWT tokens and handle 401 refresh logic.

---

### Types: ApplicationStatus, JobApplication interface, STATUS_LABELS, STATUS_COLORS

**File:** `frontend/src/types/index.ts`

**ApplicationStatus const object:**
```typescript
export const ApplicationStatus = {
  Applied: 0,
  PhoneScreen: 1,
  Interview: 2,
  TechnicalTest: 3,
  FinalInterview: 4,
  OfferReceived: 5,
  OfferAccepted: 6,
  Rejected: 7,
  Withdrawn: 8,
} as const;
```

`as const` makes all values readonly literals (the TypeScript compiler treats `0` as the literal type `0`, not the wide type `number`). The `ApplicationStatus` type is then derived from the values: `type ApplicationStatus = 0 | 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8`.

**IMPORTANT — name mismatch with backend:** Backend `Saved=0` is displayed as frontend `Applied=0`. Backend `Applied=1` is displayed as `PhoneScreen=1`. The integer values 0–8 match what the backend stores and returns — the names are just the frontend's UI labels.

**STATUS_LABELS:** Maps each numeric status to a human-readable string:
- 0 → "Applied", 1 → "Phone Screen", 2 → "Interview", 3 → "Technical Test", 4 → "Final Interview", 5 → "Offer Received", 6 → "Offer Accepted", 7 → "Rejected", 8 → "Withdrawn"

**STATUS_COLORS:** Maps each status to Tailwind CSS classes for badge colors:
- 0 (Applied) → `bg-blue-100 text-blue-800`
- 1 (PhoneScreen) → `bg-yellow-100 text-yellow-800`
- 2 (Interview) → `bg-purple-100 text-purple-800`
- 3 (TechnicalTest) → `bg-orange-100 text-orange-800`
- 4 (FinalInterview) → `bg-indigo-100 text-indigo-800`
- 5 (OfferReceived) → `bg-green-100 text-green-800`
- 6 (OfferAccepted) → `bg-emerald-100 text-emerald-800`
- 7 (Rejected) → `bg-red-100 text-red-800`
- 8 (Withdrawn) → `bg-gray-100 text-gray-800`

**JobApplication interface:**
```typescript
export interface JobApplication {
  id: string;
  userId: string;
  title: string;
  companyId: string;
  companyName: string;
  jobUrl?: string;
  description?: string;
  status: ApplicationStatus;
  source: string;
  createdAt: string;
  appliedAt?: string | null;
  updatedAt?: string | null;
  notes?: string;
}
```

Dates are `string` (ISO 8601 format) because JSON has no native Date type. The `?` suffix marks optional properties. `appliedAt?: string | null` means it can be absent, null, or a date string — this is important to avoid `new Date(null)` which returns the Unix epoch (Jan 1, 1970).

**AuthResponse interface:**
```typescript
export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
}
```

---

### Context: AuthContext

**File:** `frontend/src/context/AuthContext.tsx`

State: `userId: string | null` — initialized from `localStorage.getItem('userId')` on first render.

`saveAuth(data, uid)` — stores `accessToken`, `refreshToken`, and `userId` in `localStorage`, then calls `setUserId(uid)`.

`login(email, password)` — POSTs to `/api/auth/login`, decodes the JWT payload via `JSON.parse(atob(accessToken.split('.')[1]))` to extract `sub` (the user GUID), then calls `saveAuth()`.

`register(fullName, email, password)` — same pattern as login but POSTs to `/api/auth/register`.

`logout()` — clears all `localStorage` keys, sets `userId` to null. React Router will redirect to `/login` because `ProtectedRoute` checks `isAuthenticated`.

`isAuthenticated: boolean` — computed as `!!userId` (double negation converts string/null to boolean).

`useAuth()` hook — wraps `useContext(AuthContext)` and throws if called outside `AuthProvider`, preventing silent bugs.

---

### Context: ThemeContext

**File:** `frontend/src/context/ThemeContext.tsx`

State: `theme: 'light' | 'dark'` — initialized from `localStorage.getItem('theme')` or `'light'` as default.

`useEffect` on `theme` changes:
- If `dark`, adds `dark` class to `document.documentElement` (`<html>` tag).
- If `light`, removes the `dark` class.
- Saves preference to `localStorage`.

This `<html>` class toggling is the mechanism for Tailwind's `dark:` variant — when `<html>` has class `dark`, all `dark:bg-gray-800` etc. classes activate.

`toggleTheme()` — switches between `'light'` and `'dark'`.

---

### Context: ToastContext

**File:** `frontend/src/context/ToastContext.tsx`

State: `toasts: Toast[]` — array of `{ id: number, message: string, type: 'error' | 'success' }`.

`addToast(message, type)` — generates an ID from `Date.now()`, appends the toast to the array, and sets a 4000ms timeout to remove it.

`showError(message)` and `showSuccess(message)` — memoized with `useCallback` to prevent unnecessary re-renders.

The `ToastProvider` renders the toast list as fixed-positioned overlays in the bottom-right corner using Tailwind classes. Error toasts are `bg-red-600`, success are `bg-green-600`.

---

### Component: App.tsx (routing)

**File:** `frontend/src/App.tsx`

```tsx
export default function App() {
  return (
    <ThemeProvider>
      <BrowserRouter basename="/jobtracker-pro">
        <ToastProvider>
          <AuthProvider>
            <Routes>
              <Route path="/login" element={<LoginPage />} />
              <Route path="/register" element={<RegisterPage />} />
              <Route path="/dashboard" element={
                <ProtectedRoute><DashboardPage /></ProtectedRoute>
              } />
              <Route path="*" element={<Navigate to="/login" replace />} />
            </Routes>
          </AuthProvider>
        </ToastProvider>
      </BrowserRouter>
    </ThemeProvider>
  );
}
```

Provider nesting order matters: `ThemeProvider` is outermost (available everywhere), then `BrowserRouter` (needed for routing hooks), then `ToastProvider`, then `AuthProvider` (needs apiClient which needs the base URL).

`basename="/jobtracker-pro"` — tells React Router that all routes are under the `/jobtracker-pro` base path. Without this, navigation on GitHub Pages would fail.

`path="*"` — wildcard catch-all. Unauthenticated users hitting any unknown URL are redirected to `/login`.

---

### Component: ProtectedRoute

**File:** `frontend/src/components/ProtectedRoute.tsx`

```tsx
export default function ProtectedRoute({ children }: { children: React.ReactNode }) {
  const { isAuthenticated } = useAuth();
  return isAuthenticated ? <>{children}</> : <Navigate to="/login" replace />;
}
```

Reads `isAuthenticated` from `AuthContext`. If true, renders the wrapped children. If false, replaces the current history entry with `/login` (the `replace` prop prevents going "back" to the protected route after redirect).

---

### Page: LoginPage

**File:** `frontend/src/pages/LoginPage.tsx`

State: `email`, `password`, `error` (inline error message), `loading` (disables button during request).

`handleSubmit` — prevents default form submission, calls `login(email, password)` from `useAuth()`, navigates to `/dashboard` on success. On error, displays "Invalid email or password."

Renders: full-screen centered card with email input, password input, submit button, and link to register page. All styled with Tailwind classes.

---

### Page: RegisterPage

**File:** `frontend/src/pages/RegisterPage.tsx`

Identical structure to `LoginPage`. Additional `fullName` field. On submit calls `register(fullName, email, password)`. Error message: "Registration failed. Email may already be in use."

---

### Page: DashboardPage (main application page)

**File:** `frontend/src/pages/DashboardPage.tsx`

Constants:
- `STALE_DAYS = 7` — days of inactivity to trigger stale warning
- `PAGE_SIZE = 12` — cards per page
- `ACTIVE_STATUSES` — `[0, 1, 2, 3, 4]` (Applied through FinalInterview) — statuses considered "in progress"

State:
- `applications: JobApplication[]` — full list from API
- `loading: boolean` — shows loading skeleton
- `showModal: boolean` — controls AddApplicationModal visibility
- `editingApp: JobApplication | null` — non-null opens EditApplicationModal
- `showChangePassword: boolean` — controls ChangePasswordModal visibility
- `filter: ApplicationStatus | 'all'` — current status filter
- `search: string` — search input value
- `page: number` — current pagination page

**Data fetching:**
- `fetchApplications()` — calls `jobApplicationsApi.getAll(userId)`, stores result in `applications`.
- `useEffect(() => { fetchApplications(); }, [])` — runs once on mount.
- `useEffect(() => { setPage(1); }, [filter, search])` — resets to page 1 whenever filter/search changes.

**CRUD handlers:**
- `handleAdd(data)` — calls API, re-fetches full list, shows success toast.
- `handleEdit(id, data)` — calls API, updates local state optimistically (mutates the matching item in `applications` array without re-fetching), shows success toast.
- `handleDelete(id)` — calls API, filters item out of local `applications` state.
- `handleStatusChange(id, status)` — calls API PUT, updates matching item status in local state.

**CSV Export (`exportCSV`):**
1. Defines header row: `['Title', 'Company', 'Status', 'Source', 'Job URL', 'Notes', 'Added', 'Applied At']`
2. Maps all applications to arrays of values.
3. Joins rows with newline and cells with comma, wrapping each cell in double quotes (escaping internal quotes as `""`).
4. Creates a `Blob` with MIME type `text/csv;charset=utf-8;`.
5. Creates a temporary anchor element, sets `href` to a `URL.createObjectURL(blob)`, triggers `.click()`, then revokes the object URL.

**Stale detection:**
```typescript
const staleApps = applications.filter(a => {
  if (!ACTIVE_STATUSES.includes(a.status)) return false;
  const lastActivity = new Date(a.updatedAt ?? a.createdAt);
  const daysSince = (Date.now() - lastActivity.getTime()) / (1000 * 60 * 60 * 24);
  return daysSince >= STALE_DAYS;
});
```
Uses `updatedAt ?? createdAt` — if `updatedAt` is null/undefined, falls back to `createdAt`. `new Date(null)` is explicitly avoided (would return epoch).

**Filtering and pagination:**
```typescript
const filtered = applications
  .filter(a => filter === 'all' || a.status === filter)
  .filter(a => {
    if (!search.trim()) return true;
    const q = search.toLowerCase();
    return a.title.toLowerCase().includes(q) || a.companyName.toLowerCase().includes(q);
  });

const totalPages = Math.max(1, Math.ceil(filtered.length / PAGE_SIZE));
const paginated = filtered.slice((page - 1) * PAGE_SIZE, page * PAGE_SIZE);
```

**Stats computation:**
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

---

### Component: JobApplicationCard

**File:** `frontend/src/components/JobApplicationCard.tsx`

Props: `application`, `onDelete(id)`, `onStatusChange(id, status)`, `onEdit(application)`.

State: `confirmDelete: boolean` — two-step delete confirmation.

Renders:
- Header row: truncated job title, company name, "Added {date}", status badge.
- Optional: job URL link (opens in new tab with `rel="noopener noreferrer"` for security).
- Optional: notes preview with `line-clamp-2` (two-line clamp via CSS).
- Action row: status dropdown (calls `onStatusChange` on change), Edit button (calls `onEdit`), Delete button.
- Delete button shows "Delete" by default. On click, switches to "Confirm" + "Cancel" buttons (two-step confirmation pattern).

All elements support dark mode via `dark:` Tailwind variants.

---

### Component: AddApplicationModal

**File:** `frontend/src/components/AddApplicationModal.tsx`

Props: `userId`, `onAdd(data)`, `onClose()`.

State: `form` object with `title`, `companyName`, `jobUrl`, `description`, `source` (default "LinkedIn"). `loading` boolean.

`handleSubmit` — calls `onAdd({ userId, ...form })` with optional fields set to `undefined` if empty. Closes modal on success.

Source options: `['LinkedIn', 'Indeed', 'Direct', 'Referral', 'Other']`.

Rendered as a fixed full-screen overlay (`fixed inset-0 bg-black/40`) with a centered card. The `bg-black/40` creates a semi-transparent backdrop.

---

### Component: EditApplicationModal

**File:** `frontend/src/components/EditApplicationModal.tsx`

Props: `application` (pre-populates form fields), `onSave(id, data)`, `onClose()`.

Form fields: `title`, `companyName`, `jobUrl`, `notes` (textarea with 3 rows).

`handleSubmit` — calls `onSave(application.id, form)` with optional fields set to `undefined` if empty strings.

---

### API module: api/jobApplications.ts

**File:** `frontend/src/api/jobApplications.ts`

```typescript
export const jobApplicationsApi = {
  getAll: (userId) => apiClient.get(`/api/jobapplications/${userId}`),
  create: (data) => apiClient.post('/api/jobapplications', data),
  updateStatus: (id, newStatus, notes?) => apiClient.put(`/api/jobapplications/${id}`, { newStatus, notes }),
  edit: (id, data) => apiClient.patch(`/api/jobapplications/${id}`, data),
  delete: (id) => apiClient.delete(`/api/jobapplications/${id}`),
};
```

All methods return Axios promises. The caller uses `await` to resolve them.

---

### Axios interceptor (token refresh queue pattern)

**File:** `frontend/src/api/client.ts`

```typescript
const API_URL = import.meta.env.VITE_API_URL ?? 'http://localhost:5086';

export const apiClient = axios.create({
  baseURL: API_URL,
  headers: { 'Content-Type': 'application/json' },
});
```

**Request interceptor** — runs before every request. Reads `accessToken` from `localStorage` and attaches it as `Authorization: Bearer <token>`.

**Response interceptor (401 handling with queue):**

Module-level state:
- `isRefreshing: boolean` — true while a refresh is in progress.
- `pendingQueue` — array of `{ resolve, reject }` callbacks for requests that arrived while refresh was in progress.

Logic on 401 response:
1. If `original._retry` is true (already retried once), reject immediately — prevents infinite loops.
2. If no tokens in `localStorage`, clear storage and redirect to `/login`.
3. If `isRefreshing` is already true (another request triggered the refresh): push a new Promise into `pendingQueue`. When the refresh completes, `drainQueue` either resolves all pending Promises with the new token (retry the original requests) or rejects them all (redirect to login).
4. Set `original._retry = true` and `isRefreshing = true`.
5. POST to `/api/auth/refresh` with current tokens.
6. On success: store new tokens, call `drainQueue(null, newToken)`, retry the original request.
7. On failure: call `drainQueue(err, null)`, clear localStorage, redirect to `/login`.
8. `finally`: reset `isRefreshing = false`.

This pattern ensures that if 5 requests all get 401 simultaneously, only one refresh call is made and all 5 original requests are retried with the new token.

---

### CSS: index.css (Tailwind v4 directives, dark mode variant)

**File:** `frontend/src/index.css`

```css
@import "tailwindcss";

/* Class-based dark mode for Tailwind v4 */
@variant dark (&:where(.dark, .dark *));
```

Two lines:
- `@import "tailwindcss"` — Tailwind v4's new import syntax. In v3 you used `@tailwind base; @tailwind components; @tailwind utilities;`. In v4 a single import does everything.
- `@variant dark (...)` — defines the `dark:` variant to activate when the element itself or any ancestor has the `dark` class. This enables class-based dark mode (as opposed to media-query-based dark mode). `ThemeContext` toggles the `dark` class on `<html>` to activate it.

---

## PART 6: TESTING

### Concept: What is xUnit?

xUnit is the standard .NET testing framework used in this project. Test methods are marked with `[Fact]` (a single test case). The framework runs all `[Fact]` methods and reports pass/fail. In async tests, methods return `Task` and use `await`. xUnit creates a new instance of the test class for each test, providing test isolation.

### Concept: What is Moq?

Moq is a .NET mocking library. A mock is a fake implementation of an interface used in unit tests. `new Mock<IUserRepository>()` creates a mock. `mock.Setup(r => r.GetByEmailAsync(...)).ReturnsAsync(user)` configures it to return a specific value. `mock.Verify(r => r.AddAsync(...), Times.Once)` asserts that a method was called exactly once. Mocks allow you to test a handler in complete isolation without a real database.

### Concept: What is FluentAssertions?

FluentAssertions is a .NET assertion library that produces readable test assertions. Instead of `Assert.Equal(expected, actual)`, you write `actual.Should().Be(expected)`. For async exceptions: `await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("Invalid email or password.")`. The fluent syntax makes test failures easier to understand.

### Concept: What is WebApplicationFactory?

`WebApplicationFactory<TEntryPoint>` is a class from `Microsoft.AspNetCore.Mvc.Testing` that spins up a full in-memory ASP.NET Core server for integration tests. You get a real `HttpClient` that makes real HTTP requests through the entire middleware pipeline (CORS, rate limiting, auth, controllers) but against an in-memory test database. `TEntryPoint` is `Program` — which is why `Program.cs` has the `public partial class Program { }` declaration.

### Concept: What is InMemory database in tests?

`UseInMemoryDatabase("JobTrackerTestDb")` configures EF Core to use an in-memory store instead of PostgreSQL. The in-memory database lives only for the duration of the test process and needs no network connection or Docker. It supports all EF Core LINQ operations but does not enforce relational constraints (foreign keys, unique indexes). The test factory uses `UseInternalServiceProvider(dbServiceProvider)` to isolate the InMemory provider from Npgsql's provider to avoid EF Core's "two providers" conflict.

---

### Class: CustomWebApplicationFactory

**File:** `tests/JobTrackerPro.IntegrationTests/CustomWebApplicationFactory.cs`

```csharp
public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
```

`IAsyncLifetime` provides `InitializeAsync()` (calls `StartServer()`) and `DisposeAsync()` (calls `base.DisposeAsync()`).

**ConfigureWebHost override:**
1. Find and remove the existing `DbContextOptions<ApplicationDbContext>` registration (the PostgreSQL one).
2. Build a separate `ServiceProvider` with only `EntityFrameworkInMemoryDatabase` (isolation from Npgsql).
3. Re-register `ApplicationDbContext` with `UseInMemoryDatabase("JobTrackerTestDb")` and `UseInternalServiceProvider(dbServiceProvider)`.
4. Set environment to "Testing" via `builder.UseEnvironment("Testing")`.

Setting "Testing" environment:
- Disables EF Core migration execution in `Program.cs`.
- Disables rate limiter registration and middleware in `Program.cs`.

This collection is registered via `IntegrationTestCollection.cs` using `[CollectionDefinition("Integration")]` and `ICollectionFixture<CustomWebApplicationFactory>`, so the factory (and its HTTP server) is shared across all tests in the "Integration" collection.

---

### Class: BaseIntegrationTest

**File:** `tests/JobTrackerPro.IntegrationTests/BaseIntegrationTest.cs`

Abstract base class for all integration tests. Provides:

- `Client: HttpClient` — created from `factory.CreateClient()`.
- `GetAuthTokenAsync(email, password)` — registers a user (or logs in if already exists) and returns the JWT access token string.
- `SetBearerToken(token)` — sets `Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token)`.
- `ResetDatabase()` — creates a new scope, resolves `ApplicationDbContext`, calls `EnsureDeleted()` then `EnsureCreated()` to reset the InMemory database between tests.

---

### Tests: CreateJobApplicationHandlerTests (unit)

**File:** `tests/JobTrackerPro.UnitTests/JobApplications/CreateJobApplicationHandlerTests.cs`

3 test cases:

1. **`Handle_WhenCompanyDoesNotExist_ShouldCreateCompanyAndApplication`** — Mocks `GetByNameAsync` to return null. Verifies `AddAsync` on both company and application repositories called once each. Verifies `SaveChangesAsync` called once.

2. **`Handle_WhenCompanyAlreadyExists_ShouldReuseCompany`** — Mocks `GetByNameAsync` to return an existing `Company`. Verifies `AddAsync` on company repository is called **zero** times (reuse). Verifies application is still saved.

3. **`Handle_ShouldReturnValidGuid`** — Verifies the returned `Guid` is not `Guid.Empty`.

Uses `NullLogger<CreateJobApplicationHandler>.Instance` — a no-op logger that discards all log messages (avoids needing a real logger in tests).

---

### Tests: LoginHandlerTests + RegisterHandlerTests (unit)

**LoginHandlerTests (3 cases):**

1. **`Handle_WithValidCredentials_ShouldReturnAuthResponse`** — Creates a user with a real BCrypt hash, mocks `GetByEmailAsync` to return it, mocks `GenerateToken` to return "fake-jwt-token". Asserts `AccessToken == "fake-jwt-token"`, `RefreshToken` non-empty, `ExpiresAt` in the future.

2. **`Handle_WithWrongPassword_ShouldThrowUnauthorizedAccessException`** — Uses `BCrypt.HashPassword("CorrectPassword!")` but submits "WrongPassword!". Asserts `ThrowAsync<UnauthorizedAccessException>` with message "Invalid email or password."

3. **`Handle_WithNonExistentEmail_ShouldThrowUnauthorizedAccessException`** — Mocks `GetByEmailAsync` to return null. Asserts throws `UnauthorizedAccessException`.

**RegisterHandlerTests (2 cases):**

1. **`Handle_WithNewEmail_ShouldReturnAuthResponse`** — Mocks `ExistsAsync` returns false. Asserts `AuthResponse` returned, `SaveChangesAsync` called once.

2. **`Handle_WithExistingEmail_ShouldThrowInvalidOperationException`** — Mocks `ExistsAsync` returns true. Asserts throws `InvalidOperationException` with message containing the duplicate email.

---

### Tests: GetJobApplicationsHandlerTests (unit)

**File:** `tests/JobTrackerPro.UnitTests/JobApplications/GetJobApplicationsHandlerTests.cs`

Constructor setup: cache mock configured to return `null` (simulates cache miss) so the test always hits the repository.

2 test cases:

1. **`Handle_WhenUserHasApplications_ShouldReturnMappedDtos`** — Creates 2 `JobApplication` entities with a shared `Company`. Mocks repository. Asserts result has count 2 and all DTOs have non-empty `Id`.

2. **`Handle_WhenUserHasNoApplications_ShouldReturnEmptyList`** — Repository returns empty list. Asserts result is empty.

---

### Tests: JobApplicationsControllerTests (integration — 12 cases)

**File:** `tests/JobTrackerPro.IntegrationTests/JobApplications/JobApplicationsControllerTests.cs`

All tests are in `[Collection("Integration")]` — share the factory instance.

| Test | Method | Expectation |
|------|--------|-------------|
| `GetJobApplications_WithoutToken_ShouldReturn401` | GET | 401 (no JWT) |
| `GetJobApplications_AfterCreating_ShouldReturnApplications` | GET after POST | 200 with 1 item |
| `GetJobApplications_ShouldReturnStatusAsInt` | GET | Status field is `int` (0 = Saved) |
| `CreateJobApplication_WithValidToken_ShouldReturn201` | POST | 201, body has `id` GUID |
| `CreateJobApplication_WithInvalidData_ShouldReturn400` | POST empty title/company | 400 |
| `CreateJobApplication_WithoutToken_ShouldReturn401` | POST | 401 |
| `UpdateStatus_WithValidId_ShouldReturn204` | PUT | 204 |
| `UpdateStatus_WithUnknownId_ShouldReturn404` | PUT | 404 |
| `Edit_WithValidId_ShouldReturn204AndPersistChanges` | PATCH + GET verify | 204; title/notes updated in DB |
| `Edit_WithUnknownId_ShouldReturn404` | PATCH | 404 |
| `Delete_WithValidId_ShouldReturn204AndRemoveApplication` | DELETE + GET verify | 204; list is empty |
| `Delete_WithUnknownId_ShouldReturn404` | DELETE | 404 |

---

### Tests: AuthControllerTests (integration — 4 cases)

**File:** `tests/JobTrackerPro.IntegrationTests/Auth/AuthControllerTests.cs`

| Test | Expectation |
|------|-------------|
| `Register_WithValidData_ShouldReturn200WithTokens` | 200; `AccessToken` non-empty; `RefreshToken` non-empty; `ExpiresAt` in future |
| `Register_WithDuplicateEmail_ShouldReturn500` | 500 (InvalidOperationException not caught as 409) |
| `Login_WithValidCredentials_ShouldReturn200WithTokens` | 200; `AccessToken` non-empty |
| `Login_WithWrongPassword_ShouldReturn401` | 401 |

Each test uses a unique email via `$"test_{Guid.NewGuid()}@jobtracker.com"` to avoid collisions within the shared in-memory database.

---

## PART 7: CI/CD & INFRASTRUCTURE

### Concept: What is GitHub Actions?

GitHub Actions is GitHub's built-in CI/CD (Continuous Integration/Continuous Deployment) platform. Workflows are defined in YAML files under `.github/workflows/`. A workflow is triggered by events (push, pull request) and consists of jobs. Each job runs on a virtual machine (runner) and executes a sequence of steps.

### Concept: What is Docker? What is a multi-stage build?

Docker packages an application and all its dependencies into a portable container image that runs identically everywhere. A multi-stage Docker build uses multiple `FROM` instructions in one Dockerfile. The first stage (build) uses a large SDK image to compile the code. The second stage (runtime) uses a small runtime image and copies only the compiled output. This produces a minimal production image without the SDK, compilers, or source files.

### Concept: What is Azure App Service?

Azure App Service is Microsoft's PaaS (Platform as a Service) for hosting web applications. You deploy code (or a Docker container) and Azure handles the server infrastructure, scaling, and SSL certificates. This project deploys the .NET API to Azure App Service using GitHub Actions and the `azure/webapps-deploy@v3` action with a publish profile.

### Concept: What is GitHub Pages? How does SPA routing work?

GitHub Pages hosts static files from a repository. The challenge with Single-Page Applications (SPAs) is that GitHub Pages only knows about one file (`index.html`) — if you navigate directly to `https://ramiro671.github.io/jobtracker-pro/dashboard`, GitHub Pages returns a 404 because there is no physical `dashboard` file.

The solution: GitHub Pages serves `404.html` for any unmatched path. The `404.html` in this project encodes the path as a query string and redirects to `index.html`. The React app's `index.html` then decodes the query string and restores the correct route. This is a well-known SPA trick for GitHub Pages.

---

### File: .github/workflows/ci.yml (every job explained)

**Trigger:** Push or PR to `main` branch.

**Environment variable:** `FORCE_JAVASCRIPT_ACTIONS_TO_NODE24: true` — opts all GitHub Actions JavaScript runners into Node.js 24.

**Job 1: build-and-test**
- Runner: `ubuntu-latest`
- Steps: checkout → setup .NET 10 → `dotnet restore` → `dotnet build --configuration Release` → run unit tests → run integration tests
- Unit tests: `dotnet test tests/JobTrackerPro.UnitTests --no-build --configuration Release`
- Integration tests: `dotnet test tests/JobTrackerPro.IntegrationTests --no-build --configuration Release`

**Job 2: docker-build**
- `needs: build-and-test` — only runs if build-and-test passes
- Steps: checkout → `docker build -t jobtracker-api:${{ github.sha }} .` → `docker images jobtracker-api`
- The `github.sha` variable is the commit SHA — used as the image tag.
- This job validates the Dockerfile but does not push to a registry.

**Job 3: deploy-api**
- `needs: docker-build`
- Only runs on push to main (not on PRs): `if: github.ref == 'refs/heads/main' && github.event_name == 'push'`
- Steps: checkout → setup .NET 10 → `dotnet publish src/JobTrackerPro.Api -c Release -o ./publish` → `azure/webapps-deploy@v3` with `AZURE_APP_NAME` and `AZURE_PUBLISH_PROFILE` secrets

**Job 4: deploy-frontend**
- `needs: build-and-test` (independent of docker-build)
- Only on push to main
- `permissions: contents: write` — required to push to `gh-pages` branch
- Steps: checkout → setup Node 22 → `npm ci && npm run build` (with `VITE_API_URL` env var from secret) → `peaceiris/actions-gh-pages@v4` deploys `./frontend/dist` to `gh-pages` branch

---

### File: Dockerfile (every stage explained)

```dockerfile
# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy only .csproj files first (Docker layer cache optimization)
COPY JobTrackerPro.slnx .
COPY src/JobTrackerPro.Domain/JobTrackerPro.Domain.csproj src/JobTrackerPro.Domain/
COPY src/JobTrackerPro.Application/JobTrackerPro.Application.csproj src/JobTrackerPro.Application/
COPY src/JobTrackerPro.Infrastructure/JobTrackerPro.Infrastructure.csproj src/JobTrackerPro.Infrastructure/
COPY src/JobTrackerPro.Api/JobTrackerPro.Api.csproj src/JobTrackerPro.Api/

# Restore NuGet packages (cached if .csproj files unchanged)
RUN dotnet restore src/JobTrackerPro.Api/JobTrackerPro.Api.csproj

# Copy source code and publish
COPY src/ src/
RUN dotnet publish src/JobTrackerPro.Api/JobTrackerPro.Api.csproj \
    -c Release -o /app/publish --no-restore

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Create non-root user for security
RUN groupadd --system appgroup && useradd --system --gid appgroup appuser

COPY --from=build /app/publish .
RUN chown -R appuser:appgroup /app
USER appuser

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "JobTrackerPro.Api.dll"]
```

**Layer caching optimization:** `.csproj` files change rarely, so copying them before source files means the `dotnet restore` layer is cached on subsequent builds if only source files changed. This significantly speeds up builds.

**Non-root user:** Running as `appuser` (not root) limits damage if a container escape vulnerability is exploited.

**`mcr.microsoft.com/dotnet/aspnet:10.0`** — Microsoft's official ASP.NET Core runtime image. Much smaller than the SDK image (no compiler toolchain). Only includes the .NET runtime.

---

### File: docker-compose.yml

Three services:

**postgres:**
- Image: `postgres:16-alpine` (lightweight Alpine-based)
- Port: 5432
- Volume: `postgres_data` (persists data across container restarts)
- Healthcheck: `pg_isready -U postgres`

**redis:**
- Image: `redis:7-alpine`
- Port: 6379
- Healthcheck: `redis-cli ping`

**api:**
- Built from local Dockerfile
- Port: 8080
- Environment variables injected for connection strings and JWT settings
- `depends_on` with `condition: service_healthy` — waits for both postgres and redis to pass their healthchecks before starting

---

### File: vite.config.ts

```typescript
import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'

export default defineConfig({
  plugins: [react(), tailwindcss()],
  base: '/jobtracker-pro/',
})
```

- `react()` plugin — enables JSX transformation and React Fast Refresh (hot module replacement for React components).
- `tailwindcss()` plugin — Tailwind CSS v4's Vite integration. Processes `@import "tailwindcss"` and utility class detection.
- `base: '/jobtracker-pro/'` — all built assets reference URLs under `/jobtracker-pro/`. Required for GitHub Pages hosting at `https://ramiro671.github.io/jobtracker-pro/`.

---

### File: frontend/public/404.html (SPA trick explained)

GitHub Pages serves this file for all 404 (not found) requests. The embedded script:

1. Reads the current URL: `window.location`.
2. Takes the path segments after the repo name (`/jobtracker-pro/`).
3. Re-encodes them as a query string parameter `?p=/remaining/path`.
4. Redirects to the root `index.html` with this encoded path.
5. React Router running in `index.html` can then decode `?p=` and navigate to the correct route.

This avoids the "404 on page refresh" problem that all GitHub Pages SPAs face, because GitHub Pages has no server-side routing — it only serves static files.

---

## PART 8: CONFIGURATION

### File: appsettings.json (every section)

**File:** `src/JobTrackerPro.Api/appsettings.json`

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.EntityFrameworkCore": "Warning",
        "System": "Warning"
      }
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=jobtracker_dev;Username=postgres;Password=postgres",
    "Redis": "localhost:6379"
  },
  "JwtSettings": {
    "Secret": "super-secret-key-for-development-only-change-in-production",
    "Issuer": "JobTrackerPro",
    "Audience": "JobTrackerPro",
    "ExpirationMinutes": 60
  },
  "ApplicationInsights": {
    "ConnectionString": ""
  },
  "EmailSettings": {
    "Enabled": false,
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "Username": "",
    "Password": "",
    "FromEmail": "noreply@jobtrackerpro.dev",
    "FromName": "JobTracker Pro"
  },
  "AllowedHosts": "*"
}
```

**Serilog section:**
- Default level: `Information` — logs all INFO, WARN, ERROR, FATAL.
- `Microsoft` namespaces overridden to `Warning` — suppresses ASP.NET Core framework info noise.
- `Microsoft.EntityFrameworkCore` overridden to `Warning` — suppresses EF Core SQL query logging.

**ConnectionStrings:**
- `DefaultConnection` — Npgsql format: `Host=...;Database=...;Username=...;Password=...`. In production (Azure App Service), set as a Connection String named `DefaultConnection` of type `Custom`.
- `Redis` — `host:port` format for StackExchange.Redis. Falls back to `localhost:6379` if not set.

**JwtSettings:**
- `Secret` — **MUST** be changed in production. At least 32 characters for security. Azure App Service env var: `JwtSettings__Secret`.
- `Issuer` and `Audience` — both "JobTrackerPro". These are validated on every incoming JWT.
- `ExpirationMinutes` — 60 (1 hour). After expiry, the client must use the refresh token.

**ApplicationInsights:** Empty connection string by default — Azure App Service provides this via environment variable.

**EmailSettings:** All disabled by default. Set `Enabled: true` and SMTP credentials to enable stale notification emails.

---

### Azure Environment Variables (double-underscore convention)

Azure App Service injects configuration as environment variables. ASP.NET Core maps `__` (double underscore) to `:` (colon) in the configuration hierarchy.

| Environment Variable | appsettings.json equivalent |
|---------------------|----------------------------|
| `JwtSettings__Secret` | `JwtSettings:Secret` |
| `JwtSettings__Issuer` | `JwtSettings:Issuer` |
| `JwtSettings__Audience` | `JwtSettings:Audience` |
| `JwtSettings__ExpirationMinutes` | `JwtSettings:ExpirationMinutes` |
| `ConnectionStrings__DefaultConnection` | `ConnectionStrings:DefaultConnection` |
| `ConnectionStrings__Redis` | `ConnectionStrings:Redis` |

The Connection String `DefaultConnection` in Azure App Service (type: Custom) is automatically accessible as `ConnectionStrings:DefaultConnection` in ASP.NET Core.

---

### GitHub Secrets Required

| Secret | Job that uses it |
|--------|-----------------|
| `AZURE_APP_NAME` | deploy-api |
| `AZURE_PUBLISH_PROFILE` | deploy-api |
| `VITE_API_URL` | deploy-frontend (injected at build time as env var, becomes `import.meta.env.VITE_API_URL` in TypeScript) |
| `GITHUB_TOKEN` | deploy-frontend (auto-provided by GitHub; used by `peaceiris/actions-gh-pages`) |

---

### Frontend .env.development / .env.production

Vite reads `.env.development` during `npm run dev` and `.env.production` during `npm run build`. Only variables prefixed with `VITE_` are exposed to the browser code via `import.meta.env`.

**Development:**
```
VITE_API_URL=http://localhost:5086
```

**Production:**
Set via GitHub Secret `VITE_API_URL` injected at build time in the CI workflow:
```yaml
env:
  VITE_API_URL: ${{ secrets.VITE_API_URL }}
```
Current production value: `https://jobtracker-api-prod-ehg6euckd4evaabw.centralus-01.azurewebsites.net`

In `client.ts`:
```typescript
const API_URL = import.meta.env.VITE_API_URL ?? 'http://localhost:5086';
```
The `??` (nullish coalescing) fallback ensures local development works even without the env file.

---

## APPENDIX: Key Architectural Decisions

**Why Clean Architecture?** Separates domain logic from infrastructure concerns. The Domain project has zero NuGet dependencies. The Application project knows nothing about EF Core or Redis. This makes unit testing fast (no DB needed) and allows swapping infrastructure (e.g., replacing PostgreSQL with SQL Server) without touching business logic.

**Why MediatR?** Decouples controllers from application logic. Controllers become thin routing wrappers. Adding a new feature means adding a new Command/Query + Handler without touching existing files (Open/Closed Principle).

**Why record types for Commands/Queries?** C# `record` types are immutable by default and support positional construction. Commands should be immutable — once created they should not change. The `with` expression (used in `JobApplicationsController.Edit`) creates a modified copy without mutating the original.

**Why int for Status in DTO?** The frontend `ApplicationStatus` const uses integer values for comparison (`a.status === filter`). If the API returned strings ("Applied", "Screening"), the filter would break. The integer representation ensures the comparison works correctly.

**Why cache invalidation on create but not on update/delete?** The `CreateJobApplicationHandler` explicitly removes the cache key. `UpdateJobApplicationHandler` and `DeleteJobApplicationHandler` do not — the DashboardPage optimistically updates local React state immediately, so the user sees the change instantly. The cache will expire after 10 minutes on its own. This is a trade-off between simplicity and strict cache consistency.

**Why `UseEnvironment("Testing")` and not `IsRelational()`?** EF Core's `IsRelational()` returns `true` even for InMemory providers when using `UseInternalServiceProvider`. The environment name check (`IsEnvironment("Testing")`) is reliable and explicit.

**Why GitHub Pages instead of Netlify for frontend hosting?** The CI/CD pipeline was updated to use `peaceiris/actions-gh-pages@v4` and GitHub Pages. The `CLAUDE.md` references Netlify historically, but the current `ci.yml` deploys to GitHub Pages at `https://ramiro671.github.io/jobtracker-pro`.
