# JobTracker Pro — Technical Deep Dive
## Code, Infrastructure, Patterns & Architecture Reference

> Full-stack application: ASP.NET Core 10 + React 18 + TypeScript + PostgreSQL + Redis + Azure + Netlify
> Architecture: Clean Architecture + CQRS + MediatR + Repository + Unit of Work

---

## 1. PROJECT STRUCTURE — FILE TREE

```
jobtracker-pro/
├── src/
│   ├── JobTrackerPro.Domain/
│   │   ├── Entities/
│   │   │   ├── JobApplication.cs   ← Aggregate root
│   │   │   ├── User.cs
│   │   │   ├── Company.cs
│   │   │   └── RefreshToken.cs
│   │   ├── Enums/
│   │   │   ├── ApplicationStatus.cs
│   │   │   ├── SeniorityLevel.cs
│   │   │   └── WorkModality.cs
│   │   ├── ValueObjects/
│   │   │   ├── TechStack.cs
│   │   │   └── DateRange.cs
│   │   └── Interfaces/
│   │       ├── IJobApplicationRepository.cs
│   │       ├── IUserRepository.cs
│   │       ├── ICompanyRepository.cs
│   │       └── IUnitOfWork.cs
│   │
│   ├── JobTrackerPro.Application/
│   │   ├── Behaviors/
│   │   │   └── ValidationBehavior.cs     ← MediatR pipeline
│   │   ├── DTOs/
│   │   │   └── JobApplicationDto.cs
│   │   ├── JobApplications/
│   │   │   ├── Commands/
│   │   │   │   ├── CreateJobApplicationCommand.cs
│   │   │   │   ├── CreateJobApplicationHandler.cs
│   │   │   │   ├── EditJobApplicationCommand.cs
│   │   │   │   ├── EditJobApplicationHandler.cs
│   │   │   │   ├── UpdateStatusCommand.cs
│   │   │   │   ├── UpdateStatusHandler.cs
│   │   │   │   ├── DeleteJobApplicationCommand.cs
│   │   │   │   └── DeleteJobApplicationHandler.cs
│   │   │   ├── Queries/
│   │   │   │   ├── GetJobApplicationsQuery.cs
│   │   │   │   └── GetJobApplicationsHandler.cs
│   │   │   └── Validators/
│   │   │       └── CreateJobApplicationValidator.cs
│   │   ├── Auth/
│   │   │   ├── Commands/ (Register, Login, Refresh, Logout)
│   │   │   └── Validators/
│   │   └── Users/
│   │       └── Commands/
│   │           ├── ChangePasswordCommand.cs
│   │           └── ChangePasswordHandler.cs
│   │
│   ├── JobTrackerPro.Infrastructure/
│   │   ├── Persistence/
│   │   │   ├── ApplicationDbContext.cs
│   │   │   └── Repositories/
│   │   │       ├── JobApplicationRepository.cs
│   │   │       ├── UserRepository.cs
│   │   │       └── CompanyRepository.cs
│   │   ├── Authentication/
│   │   │   └── JwtTokenGenerator.cs
│   │   ├── Caching/
│   │   │   └── RedisCacheService.cs
│   │   ├── Email/
│   │   │   ├── EmailSettings.cs
│   │   │   └── SmtpEmailService.cs
│   │   ├── BackgroundServices/
│   │   │   └── StaleNotificationService.cs
│   │   ├── Migrations/
│   │   │   ├── 20260309051853_InitialCreate.cs
│   │   │   └── 20260310204655_AddRefreshTokens.cs
│   │   └── DependencyInjection.cs
│   │
│   └── JobTrackerPro.Api/
│       ├── Controllers/
│       │   ├── JobApplicationsController.cs
│       │   ├── AuthController.cs
│       │   └── UsersController.cs
│       ├── Middleware/
│       │   └── ExceptionHandlingMiddleware.cs
│       └── Program.cs
│
├── tests/
│   ├── JobTrackerPro.UnitTests/
│   │   ├── JobApplicationTests.cs       ← Domain entity tests
│   │   └── CreateJobApplicationHandlerTests.cs
│   └── JobTrackerPro.IntegrationTests/
│       ├── CustomWebApplicationFactory.cs
│       └── JobApplicationsControllerTests.cs
│
└── frontend/src/
    ├── api/
    │   ├── client.ts              ← Axios + interceptor
    │   └── jobApplications.ts     ← All HTTP calls
    ├── components/
    │   ├── JobApplicationCard.tsx
    │   ├── AddApplicationModal.tsx
    │   ├── EditApplicationModal.tsx
    │   ├── ChangePasswordModal.tsx
    │   └── ProtectedRoute.tsx
    ├── context/
    │   ├── AuthContext.tsx
    │   ├── ToastContext.tsx
    │   └── ThemeContext.tsx
    ├── pages/
    │   ├── LoginPage.tsx
    │   ├── RegisterPage.tsx
    │   └── DashboardPage.tsx
    └── types/index.ts
```

---

## 2. CLEAN ARCHITECTURE — DEPENDENCY RULE IN CODE

### The Dependency Rule
```
Domain      → depends on: nothing (0 NuGet refs to external libs)
Application → depends on: Domain
Infrastructure → depends on: Domain + Application (implements interfaces)
Api         → depends on: Application + Infrastructure (for DI wiring)
```

### How it looks in .csproj files

```xml
<!-- Domain.csproj — zero external dependencies -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
  </PropertyGroup>
  <!-- NO PackageReference here. Pure C#. -->
</Project>

<!-- Application.csproj — only Domain + MediatR abstractions -->
<ItemGroup>
  <ProjectReference Include="..\JobTrackerPro.Domain\JobTrackerPro.Domain.csproj" />
  <PackageReference Include="MediatR" Version="12.*" />
  <PackageReference Include="FluentValidation" Version="11.*" />
</ItemGroup>

<!-- Infrastructure.csproj — knows about DB, Redis, JWT -->
<ItemGroup>
  <ProjectReference Include="..\JobTrackerPro.Domain\..." />
  <ProjectReference Include="..\JobTrackerPro.Application\..." />
  <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="9.*" />
  <PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="9.*" />
  <PackageReference Include="Microsoft.Extensions.Caching.StackExchangeRedis" Version="9.*" />
  <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="9.*" />
  <PackageReference Include="BCrypt.Net-Next" Version="4.*" />
</ItemGroup>
```

---

## 3. DOMAIN LAYER — ENTITIES & VALUE OBJECTS

### JobApplication Entity (Aggregate Root)

```csharp
// Full implementation
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

    private JobApplication() { }  // For EF Core

    public static JobApplication Create(
        Guid userId, string title, Guid companyId,
        string? jobUrl = null, string? description = null, string? source = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Job title is required.", nameof(title));

        return new JobApplication
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = title.Trim(),
            CompanyId = companyId,
            JobUrl = jobUrl?.Trim(),
            Description = description?.Trim(),
            Source = source?.Trim(),
            Status = ApplicationStatus.Saved,
            CreatedAt = DateTime.UtcNow
        };
    }

    // Domain behavior: auto-set AppliedAt on first transition to Applied
    public void UpdateStatus(ApplicationStatus newStatus, string? notes = null)
    {
        Status = newStatus;
        if (notes is not null) Notes = notes;
        UpdatedAt = DateTime.UtcNow;
        if (newStatus == ApplicationStatus.Applied && !AppliedAt.HasValue)
            AppliedAt = DateTime.UtcNow;
    }

    public void UpdateDetails(string title, string? jobUrl, string? notes)
    {
        if (!string.IsNullOrWhiteSpace(title)) Title = title.Trim();
        JobUrl = jobUrl?.Trim();
        if (notes is not null) Notes = notes;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetTechStack(TechStack techStack)
    {
        TechStack = techStack ?? throw new ArgumentNullException(nameof(techStack));
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetSalary(decimal? min, decimal? max, string? currency)
    {
        if (min.HasValue && max.HasValue && min > max)
            throw new ArgumentException("Minimum salary cannot exceed maximum.");
        SalaryMin = min; SalaryMax = max;
        SalaryCurrency = currency?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetContact(string? name, string? email)
    {
        ContactName = name?.Trim();
        ContactEmail = email?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }
}
```

### TechStack Value Object

```csharp
// Value Object: no identity, equality by value, immutable
public class TechStack
{
    public string Backend { get; private set; } = string.Empty;
    public string Frontend { get; private set; } = string.Empty;
    public string Databases { get; private set; } = string.Empty;
    public string CloudAndDevOps { get; private set; } = string.Empty;
    public string Testing { get; private set; } = string.Empty;

    private TechStack() { }  // EF Core

    public static TechStack Create(
        string backend = "", string frontend = "",
        string databases = "", string cloudAndDevOps = "", string testing = "")
        => new TechStack
        {
            Backend = backend.Trim(), Frontend = frontend.Trim(),
            Databases = databases.Trim(), CloudAndDevOps = cloudAndDevOps.Trim(),
            Testing = testing.Trim()
        };

    // For legacy Silver layer format from LinkedInAgent.Grpc
    public static TechStack FromFlatString(string technologies, string frameworks)
        => new TechStack { Backend = technologies.Trim(), Frontend = frameworks.Trim() };
}
```

### How TechStack is stored in PostgreSQL (Owned Entity)

```csharp
// In ApplicationDbContext.OnModelCreating():
builder.Entity<JobApplication>().OwnsOne(a => a.TechStack, ts =>
{
    ts.Property(t => t.Backend)
      .HasColumnName("TechStack_Backend")
      .HasMaxLength(500);
    ts.Property(t => t.Frontend)
      .HasColumnName("TechStack_Frontend")
      .HasMaxLength(500);
    // ... same for Databases, CloudAndDevOps, Testing
});
```

Generated SQL columns: `TechStack_Backend`, `TechStack_Frontend`, `TechStack_Databases`, `TechStack_CloudAndDevOps`, `TechStack_Testing` — all in the same `JobApplications` table (no JOIN needed).

---

## 4. APPLICATION LAYER — CQRS + MEDIATR

### CQRS Pattern

```
Commands (write)           Queries (read)
─────────────────          ──────────────────
CreateJobApplication  →    GetJobApplications
UpdateStatus          →    (future: GetById, GetStats)
EditJobApplication
DeleteJobApplication
ChangePassword
Register / Login
Refresh / Logout
```

### Command Example — Full Implementation

```csharp
// 1. The Command (data carrier, no logic)
public record CreateJobApplicationCommand(
    Guid UserId,
    string Title,
    string CompanyName,
    string? JobUrl = null,
    string? Description = null,
    string? Source = null,
    string TechStackBackend = "",
    string TechStackFrontend = ""
) : IRequest<JobApplicationDto>;

// 2. The Validator (runs BEFORE the handler via pipeline behavior)
public class CreateJobApplicationValidator
    : AbstractValidator<CreateJobApplicationCommand>
{
    public CreateJobApplicationValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.CompanyName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.JobUrl)
            .MaximumLength(500)
            .Must(url => string.IsNullOrEmpty(url) || Uri.IsWellFormedUriString(url, UriKind.Absolute))
            .WithMessage("JobUrl must be a valid URL.");
    }
}

// 3. The Handler (orchestrates domain + infrastructure)
public class CreateJobApplicationHandler
    : IRequestHandler<CreateJobApplicationCommand, JobApplicationDto>
{
    private readonly IJobApplicationRepository _repo;
    private readonly ICompanyRepository _companyRepo;
    private readonly IUnitOfWork _uow;

    public async Task<JobApplicationDto> Handle(
        CreateJobApplicationCommand request, CancellationToken ct)
    {
        // Find or create company
        var company = await _companyRepo.GetByNameAsync(request.CompanyName, ct)
                      ?? Company.Create(request.CompanyName);
        if (company.Id == Guid.Empty)
            await _companyRepo.AddAsync(company, ct);

        // Create domain entity
        var app = JobApplication.Create(
            request.UserId, request.Title, company.Id,
            request.JobUrl, request.Description, request.Source);

        // Set tech stack if provided
        if (!string.IsNullOrWhiteSpace(request.TechStackBackend))
            app.SetTechStack(TechStack.Create(
                backend: request.TechStackBackend,
                frontend: request.TechStackFrontend));

        await _repo.AddAsync(app, ct);
        await _uow.SaveChangesAsync(ct);  // Single SQL transaction

        return MapToDto(app, company.Name);
    }

    private static JobApplicationDto MapToDto(JobApplication a, string companyName)
        => new(
            Id: a.Id,
            Title: a.Title,
            CompanyName: companyName,
            Status: (int)a.Status,
            WorkModality: a.WorkModality.ToString(),
            SeniorityLevel: a.SeniorityLevel.ToString(),
            JobUrl: a.JobUrl,
            Source: a.Source,
            SalaryMin: a.SalaryMin,
            SalaryMax: a.SalaryMax,
            SalaryCurrency: a.SalaryCurrency,
            Notes: a.Notes,
            CreatedAt: a.CreatedAt,
            AppliedAt: a.AppliedAt,
            UpdatedAt: a.UpdatedAt
        );
}
```

### MediatR Pipeline Behavior (ValidationBehavior)

```csharp
// Runs automatically for every IRequest<T> that has a registered validator
public class ValidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        if (!_validators.Any()) return await next();

        var context = new ValidationContext<TRequest>(request);
        var failures = _validators
            .Select(v => v.Validate(context))
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Count != 0)
            throw new ValidationException(failures);
            // → ExceptionHandlingMiddleware catches this → 400 Bad Request

        return await next();
        // → Goes to the actual Handler
    }
}

// Registered in Application/DependencyInjection.cs:
services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(typeof(ApplicationAssemblyMarker).Assembly);
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
});
```

---

## 5. INFRASTRUCTURE LAYER

### ApplicationDbContext

```csharp
public class ApplicationDbContext : DbContext
{
    public DbSet<JobApplication> JobApplications => Set<JobApplication>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // JobApplication configuration
        builder.Entity<JobApplication>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Title).HasMaxLength(200).IsRequired();
            entity.Property(a => a.Description).HasColumnType("text");
            entity.Property(a => a.Status).HasConversion<int>(); // Enum → int in DB

            // TechStack as Owned Entity (no separate table, no JOIN)
            entity.OwnsOne(a => a.TechStack, ts =>
            {
                ts.Property(t => t.Backend)
                  .HasColumnName("TechStack_Backend").HasMaxLength(500);
                ts.Property(t => t.Frontend)
                  .HasColumnName("TechStack_Frontend").HasMaxLength(500);
                ts.Property(t => t.Databases)
                  .HasColumnName("TechStack_Databases").HasMaxLength(500);
                ts.Property(t => t.CloudAndDevOps)
                  .HasColumnName("TechStack_CloudAndDevOps").HasMaxLength(500);
                ts.Property(t => t.Testing)
                  .HasColumnName("TechStack_Testing").HasMaxLength(500);
            });

            // Relationship: JobApplication → Company (N:1)
            entity.HasOne(a => a.Company)
                  .WithMany()
                  .HasForeignKey(a => a.CompanyId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Index for fast user queries
            entity.HasIndex(a => a.UserId);
        });

        // User configuration
        builder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Email).HasMaxLength(200).IsRequired();
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.PasswordHash).HasColumnType("text").IsRequired();
        });

        // RefreshToken
        builder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(rt => rt.Id);
            entity.HasOne<User>()
                  .WithMany()
                  .HasForeignKey(rt => rt.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
```

### Repository Implementation

```csharp
public class JobApplicationRepository : IJobApplicationRepository
{
    private readonly ApplicationDbContext _db;

    public async Task<List<JobApplication>> GetByUserIdAsync(
        Guid userId, CancellationToken ct = default)
        => await _db.JobApplications
            .Include(a => a.Company)        // Eager load — avoids N+1
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(ct);

    public async Task AddAsync(JobApplication app, CancellationToken ct = default)
        => await _db.JobApplications.AddAsync(app, ct);
        // Note: does NOT call SaveChanges — that's UnitOfWork's job

    public async Task<JobApplication?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.JobApplications
            .Include(a => a.Company)
            .FirstOrDefaultAsync(a => a.Id == id, ct);

    public void Remove(JobApplication app)
        => _db.JobApplications.Remove(app);
        // Synchronous — EF Core tracking, no async needed
}

// Unit of Work: wraps SaveChanges
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _db;
    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        => await _db.SaveChangesAsync(ct);
}
```

### JWT Token Generator

```csharp
public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly JwtSettings _settings;

    public string GenerateToken(Guid userId, string email, string fullName)
    {
        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret)),
            SecurityAlgorithms.HmacSha256
        );

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(JwtRegisteredClaimNames.Name, fullName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_settings.ExpiryMinutes), // 60 min
            signingCredentials: signingCredentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public (bool IsValid, Guid UserId) ValidateRefreshCandidate(string accessToken)
    {
        // Used to extract userId from an EXPIRED access token during refresh flow
        var tokenHandler = new JwtSecurityTokenHandler();
        var validationParams = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret)),
            ValidateIssuer = true, ValidIssuer = _settings.Issuer,
            ValidateAudience = true, ValidAudience = _settings.Audience,
            ValidateLifetime = false  // ← key: allows reading expired tokens
        };
        // ...
    }
}
```

### Redis Cache Service (with graceful fallback)

```csharp
public class RedisCacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<RedisCacheService> _logger;

    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        try
        {
            var json = await _cache.GetStringAsync(key, ct);
            return json is null ? default : JsonSerializer.Deserialize<T>(json);
        }
        catch (Exception ex)
        {
            // If Redis is down, log warning and continue WITHOUT cache
            // This keeps the app working even if Redis is unavailable
            _logger.LogWarning(ex, "Cache GET failed for key {Key}. Continuing without cache.", key);
            return default;  // ← graceful fallback
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken ct = default)
    {
        try
        {
            var json = JsonSerializer.Serialize(value);
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiry ?? TimeSpan.FromMinutes(10)
            };
            await _cache.SetStringAsync(key, json, options, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cache SET failed for key {Key}. Skipping cache write.", key);
        }
    }
}
```

### StaleNotificationService (BackgroundService)

```csharp
// Singleton lifetime (BackgroundService always is)
public class StaleNotificationService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;  // ← key: needed to resolve Scoped services
    private readonly ILogger<StaleNotificationService> _logger;
    private const int StaleDays = 7;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try { await CheckAndNotifyAsync(stoppingToken); }
            catch (Exception ex) { _logger.LogError(ex, "Stale notification cycle failed."); }

            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
        }
    }

    private async Task CheckAndNotifyAsync(CancellationToken ct)
    {
        // DbContext is SCOPED — must create a scope here (BackgroundService is SINGLETON)
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

        var cutoff = DateTime.UtcNow.AddDays(-StaleDays);
        var activeStatuses = new[]
        {
            ApplicationStatus.Saved, ApplicationStatus.Applied,
            ApplicationStatus.Screening, ApplicationStatus.TechnicalTest,
            ApplicationStatus.Interview
        };

        var staleApps = await db.JobApplications
            .Include(a => a.Company)
            .Where(a => activeStatuses.Contains(a.Status))
            .Where(a => (a.UpdatedAt ?? a.CreatedAt) < cutoff)
            .ToListAsync(ct);

        if (!staleApps.Any()) return;

        // Fetch users separately (no navigation property on JobApplication)
        var userIds = staleApps.Select(a => a.UserId).Distinct().ToList();
        var users = await db.Users
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, ct);

        // Group by user → 1 email per user
        foreach (var group in staleApps.GroupBy(a => a.UserId))
        {
            if (!users.TryGetValue(group.Key, out var user)) continue;

            var items = string.Join("", group.Select(a =>
                $"<li>{a.Title} at {a.Company.Name} — {a.Status}</li>"));
            var body = $"<p>The following applications have had no activity in {StaleDays}+ days:</p><ul>{items}</ul>";

            await emailService.SendAsync(user.Email, "Stale Applications Reminder", body, ct);
        }
    }
}
```

---

## 6. API LAYER

### Program.cs — Middleware Order

```csharp
var builder = WebApplication.CreateBuilder(args);

// ── Services Registration ──
builder.Services
    .AddInfrastructure(builder.Configuration)   // EF Core, Redis, JWT, Repos
    .AddApplication()                            // MediatR, Validators, Behaviors
    .AddControllers()
    .AddJsonOptions(o =>
        o.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull);

// Rate Limiting
builder.Services.AddRateLimiter(o =>
{
    o.AddFixedWindowLimiter("api", opts =>
    {
        opts.PermitLimit = 60;
        opts.Window = TimeSpan.FromMinutes(1);
        opts.QueueLimit = 0;
    });
    o.AddFixedWindowLimiter("auth", opts =>
    {
        opts.PermitLimit = 10;
        opts.Window = TimeSpan.FromMinutes(1);
    });
    o.OnRejected = async (context, ct) =>
    {
        context.HttpContext.Response.StatusCode = 429;
        await context.HttpContext.Response.WriteAsJsonAsync(
            new { error = "Too many requests. Please slow down." }, ct);
    };
});

// CORS
builder.Services.AddCors(o => o.AddPolicy("Frontend", policy =>
    policy.WithOrigins(
        "http://localhost:5173",
        "https://gleaming-lollipop-3b4183.netlify.app"
    ).AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

// ── Middleware Pipeline (ORDER MATTERS) ──
app.UseSerilogRequestLogging();                           // 1. Log all requests
app.UseCors("Frontend");                                  // 2. CORS before everything
app.UseRateLimiter();                                     // 3. Rate limit
app.UseMiddleware<ExceptionHandlingMiddleware>();          // 4. Global error handler
app.UseSwagger(); app.UseSwaggerUI(o => o.RoutePrefix = "");  // 5. Swagger at root
app.UseHttpsRedirection();                                // 6. HTTP → HTTPS
app.UseAuthentication();                                  // 7. Validate JWT
app.UseAuthorization();                                   // 8. Check [Authorize]
app.MapControllers();                                     // 9. Route to controllers

// Auto-apply migrations on startup (except in Testing environment)
if (!app.Environment.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

app.Run();
```

### Exception Handling Middleware

```csharp
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            // FluentValidation failures → 400 Bad Request
            context.Response.StatusCode = 400;
            await context.Response.WriteAsJsonAsync(new
            {
                title = "Validation failed",
                status = 400,
                errors = ex.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            // Wrong password, invalid token, etc. → 401
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new
            {
                title = "Unauthorized", status = 401, detail = ex.Message
            });
        }
        catch (KeyNotFoundException ex)
        {
            // Entity not found → 404
            context.Response.StatusCode = 404;
            await context.Response.WriteAsJsonAsync(new
            {
                title = "Not Found", status = 404, detail = ex.Message
            });
        }
        catch (Exception ex)
        {
            // Any other exception → 500
            _logger.LogError(ex, "Unhandled exception on {Path}", context.Request.Path);
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new
            {
                title = "Internal Server Error", status = 500
            });
        }
    }
}
```

### Controller Pattern

```csharp
[ApiController]
[Route("api/jobapplications")]
[Authorize]                        // All endpoints require valid JWT
[EnableRateLimiting("api")]
public class JobApplicationsController : ControllerBase
{
    private readonly ISender _sender;  // MediatR ISender — only sends, doesn't publish

    // GET /api/jobapplications/{userId}
    [HttpGet("{userId:guid}")]
    public async Task<IActionResult> GetAll(Guid userId, CancellationToken ct)
    {
        var result = await _sender.Send(new GetJobApplicationsQuery(userId), ct);
        return Ok(result);
    }

    // POST /api/jobapplications
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateJobApplicationRequest req, CancellationToken ct)
    {
        var command = new CreateJobApplicationCommand(
            req.UserId, req.Title, req.CompanyName, req.JobUrl, req.Description, req.Source);
        var result = await _sender.Send(command, ct);
        return CreatedAtAction(nameof(GetAll), new { userId = req.UserId }, result);
    }

    // PUT /api/jobapplications/{id} — update status
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateStatus(
        Guid id, [FromBody] UpdateStatusRequest req, CancellationToken ct)
    {
        var result = await _sender.Send(
            new UpdateStatusCommand(id, (ApplicationStatus)req.Status, req.Notes), ct);
        return result ? NoContent() : NotFound();
    }

    // PATCH /api/jobapplications/{id} — edit editable fields
    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> Edit(
        Guid id, [FromBody] EditJobApplicationRequest req, CancellationToken ct)
    {
        var command = new EditJobApplicationCommand(id, req.Title, req.JobUrl, req.Notes);
        var result = await _sender.Send(command, ct);
        return result ? NoContent() : NotFound();
    }

    // DELETE /api/jobapplications/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await _sender.Send(new DeleteJobApplicationCommand(id), ct);
        return result ? NoContent() : NotFound();
    }
}
```

---

## 7. FRONTEND — REACT + TYPESCRIPT

### TypeScript Interface & Enum

```typescript
// types/index.ts

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
  notes?: string;
  createdAt: string;
  appliedAt?: string;
  updatedAt?: string;
}

// Frontend enum — DIFFERENT from backend enum (intentional)
// Backend:  Saved=0, Applied=1, Screening=2, TechnicalTest=3, Interview=4...
// Frontend: Applied=0, PhoneScreen=1, Interview=2, TechnicalTest=3...
// The DTO returns int → frontend interprets it with ITS OWN values
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

export type ApplicationStatusType = typeof ApplicationStatus[keyof typeof ApplicationStatus];

export const STATUS_LABELS: Record<number, string> = {
  [ApplicationStatus.Applied]: 'Applied',
  [ApplicationStatus.PhoneScreen]: 'Phone Screen',
  [ApplicationStatus.Interview]: 'Interview',
  [ApplicationStatus.TechnicalTest]: 'Technical Test',
  [ApplicationStatus.FinalInterview]: 'Final Interview',
  [ApplicationStatus.OfferReceived]: 'Offer Received',
  [ApplicationStatus.OfferAccepted]: 'Offer Accepted',
  [ApplicationStatus.Rejected]: 'Rejected',
  [ApplicationStatus.Withdrawn]: 'Withdrawn',
};
```

### Axios Client with 401 Interceptor (Queue Pattern)

```typescript
// api/client.ts

import axios from 'axios';

const API_URL = import.meta.env.VITE_API_URL ?? 'http://localhost:5000';

export const apiClient = axios.create({ baseURL: API_URL });

// Request interceptor: attach JWT to every request
apiClient.interceptors.request.use((config) => {
  const token = localStorage.getItem('accessToken');
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

// Queue for requests that arrive while refresh is in-flight
let isRefreshing = false;
let pendingQueue: Array<{
  resolve: (token: string) => void;
  reject: (err: unknown) => void;
}> = [];

const drainQueue = (err: unknown, token: string | null) => {
  pendingQueue.forEach(p => err ? p.reject(err) : p.resolve(token!));
  pendingQueue = [];
};

// Response interceptor: handle 401
apiClient.interceptors.response.use(
  response => response,  // pass-through on success
  async (error) => {
    const config = error.config;

    // Only handle 401 once per request (_retry flag)
    if (error.response?.status !== 401 || config._retry) throw error;

    if (isRefreshing) {
      // Refresh already in progress → queue this request
      return new Promise((resolve, reject) => {
        pendingQueue.push({ resolve, reject });
      }).then(token => {
        config.headers.Authorization = `Bearer ${token}`;
        return apiClient(config);
      });
    }

    config._retry = true;
    isRefreshing = true;

    try {
      const refreshToken = localStorage.getItem('refreshToken');
      const { data } = await axios.post(`${API_URL}/api/auth/refresh`, { refreshToken });
      const newToken: string = data.accessToken;

      localStorage.setItem('accessToken', newToken);
      if (data.refreshToken) localStorage.setItem('refreshToken', data.refreshToken);

      apiClient.defaults.headers.common.Authorization = `Bearer ${newToken}`;
      drainQueue(null, newToken);        // ← unblock all queued requests

      config.headers.Authorization = `Bearer ${newToken}`;
      return apiClient(config);         // ← retry the original request
    } catch (refreshError) {
      drainQueue(refreshError, null);   // ← reject all queued requests
      localStorage.removeItem('accessToken');
      localStorage.removeItem('refreshToken');
      localStorage.removeItem('userId');
      window.location.href = '/login';
      throw refreshError;
    } finally {
      isRefreshing = false;
    }
  }
);
```

### Context API Pattern

```typescript
// context/AuthContext.tsx

interface AuthContextType {
  userId: string | null;
  login: (token: string, userId: string, refreshToken?: string) => void;
  logout: () => void;
}

const AuthContext = createContext<AuthContextType | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [userId, setUserId] = useState<string | null>(
    () => localStorage.getItem('userId')  // ← lazy initial state from localStorage
  );

  const login = (token: string, id: string, refreshToken?: string) => {
    localStorage.setItem('accessToken', token);
    localStorage.setItem('userId', id);
    if (refreshToken) localStorage.setItem('refreshToken', refreshToken);
    setUserId(id);
  };

  const logout = () => {
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('userId');
    setUserId(null);
  };

  return (
    <AuthContext.Provider value={{ userId, login, logout }}>
      {children}
    </AuthContext.Provider>
  );
}

export const useAuth = () => {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth must be used inside AuthProvider');
  return ctx;
};
```

### Dashboard: Filtering, Pagination, Stale Detection

```typescript
// DashboardPage.tsx — core logic

const PAGE_SIZE = 12;
const STALE_DAYS = 7;
const ACTIVE_STATUSES = [
  ApplicationStatus.Applied,
  ApplicationStatus.PhoneScreen,
  ApplicationStatus.Interview,
  ApplicationStatus.TechnicalTest,
  ApplicationStatus.FinalInterview,
];

// Filtering pipeline (pure functions, no side effects)
const filtered = useMemo(() => {
  return applications
    .filter(a => statusFilter === 'all' || a.status === statusFilter)
    .filter(a => {
      if (!searchTerm.trim()) return true;
      const q = searchTerm.toLowerCase();
      return (
        a.title.toLowerCase().includes(q) ||
        a.companyName.toLowerCase().includes(q)
      );
    });
}, [applications, statusFilter, searchTerm]);

// Pagination
const totalPages = Math.ceil(filtered.length / PAGE_SIZE);
const paged = filtered.slice((page - 1) * PAGE_SIZE, page * PAGE_SIZE);

// Stale detection (computed, no API call)
const staleApps = useMemo(() => {
  const cutoff = Date.now() - STALE_DAYS * 24 * 60 * 60 * 1000;
  return applications.filter(a => {
    if (!ACTIVE_STATUSES.includes(a.status)) return false;
    const lastActivity = new Date(a.updatedAt ?? a.createdAt).getTime();
    return lastActivity < cutoff;
  });
}, [applications]);

// Stats
const stats = useMemo(() => ({
  total: applications.length,
  active: applications.filter(a =>
    ACTIVE_STATUSES.includes(a.status)).length,
  offers: applications.filter(a =>
    a.status === ApplicationStatus.OfferReceived ||
    a.status === ApplicationStatus.OfferAccepted).length,
}), [applications]);

// Reset page when filters change
useEffect(() => { setPage(1); }, [statusFilter, searchTerm]);
```

### CSV Export (No Dependencies)

```typescript
const exportCSV = () => {
  const headers = ['Title', 'Company', 'Status', 'URL', 'Source', 'Notes', 'Created At'];

  const rows = applications.map(a => [
    a.title,
    a.companyName,
    STATUS_LABELS[a.status] ?? String(a.status),
    a.jobUrl ?? '',
    a.source ?? '',
    (a.notes ?? '').replace(/\n/g, ' '),
    new Date(a.createdAt).toLocaleDateString(),
  ]);

  // RFC 4180 CSV: values with commas/quotes/newlines must be quoted
  const escape = (val: string) => `"${val.replace(/"/g, '""')}"`;

  const csv = [
    headers.map(escape).join(','),
    ...rows.map(row => row.map(escape).join(','))
  ].join('\r\n');  // CRLF per RFC 4180

  const blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' });
  const url = URL.createObjectURL(blob);
  const link = document.createElement('a');
  link.href = url;
  link.download = `jobtracker-${new Date().toISOString().slice(0, 10)}.csv`;
  link.click();
  URL.revokeObjectURL(url);  // ← free memory
};
```

---

## 8. TESTING

### Unit Test — Domain Entity

```csharp
public class JobApplicationTests
{
    [Fact]
    public void UpdateStatus_ToApplied_ShouldSetAppliedAt()
    {
        // Arrange
        var app = JobApplication.Create(Guid.NewGuid(), "Dev", Guid.NewGuid());
        Assert.Null(app.AppliedAt);

        // Act
        app.UpdateStatus(ApplicationStatus.Applied);

        // Assert
        Assert.Equal(ApplicationStatus.Applied, app.Status);
        Assert.NotNull(app.AppliedAt);
        Assert.NotNull(app.UpdatedAt);
    }

    [Fact]
    public void UpdateStatus_ToApplied_Twice_ShouldNotOverwriteAppliedAt()
    {
        var app = JobApplication.Create(Guid.NewGuid(), "Dev", Guid.NewGuid());
        app.UpdateStatus(ApplicationStatus.Applied);
        var firstAppliedAt = app.AppliedAt;

        // Act: transition away and back to Applied
        app.UpdateStatus(ApplicationStatus.Interview);
        app.UpdateStatus(ApplicationStatus.Applied);

        // Assert: AppliedAt stays as the FIRST time
        Assert.Equal(firstAppliedAt, app.AppliedAt);
    }

    [Fact]
    public void Create_WithEmptyTitle_ShouldThrowArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            JobApplication.Create(Guid.NewGuid(), "   ", Guid.NewGuid()));
    }

    [Fact]
    public void SetSalary_WithMinGreaterThanMax_ShouldThrow()
    {
        var app = JobApplication.Create(Guid.NewGuid(), "Dev", Guid.NewGuid());
        Assert.Throws<ArgumentException>(() =>
            app.SetSalary(min: 100_000, max: 50_000, currency: "USD"));
    }
}
```

### Integration Test — HTTP Pipeline

```csharp
// Uses CustomWebApplicationFactory with InMemory EF + Test environment
public class JobApplicationsControllerTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    // Helper: register + login → returns authenticated HttpClient + userId
    private async Task<(HttpClient Client, Guid UserId)> CreateAuthenticatedClientAsync()
    {
        var client = _factory.CreateClient();
        var email = $"test-{Guid.NewGuid()}@test.com";

        // Register
        var regResponse = await client.PostAsJsonAsync("/api/auth/register", new
        {
            FullName = "Test User", Email = email, Password = "Test1234!"
        });
        regResponse.EnsureSuccessStatusCode();
        var regData = await regResponse.Content.ReadFromJsonAsync<JsonElement>();

        // Parse userId from JWT payload
        var token = regData.GetProperty("accessToken").GetString()!;
        var payload = token.Split('.')[1];
        var padded = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
        var json = JsonSerializer.Deserialize<JsonElement>(
            Convert.FromBase64String(padded));
        var userId = Guid.Parse(json.GetProperty("sub").GetString()!);

        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        return (client, userId);
    }

    [Fact]
    public async Task CreateJobApplication_WithValidToken_ShouldReturn201()
    {
        var (client, userId) = await CreateAuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync("/api/jobapplications", new
        {
            UserId = userId,
            Title = "Senior .NET Developer",
            CompanyName = "Acme Corp",
            JobUrl = "https://example.com/job"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Senior .NET Developer", body.GetProperty("title").GetString());
        Assert.Equal("Acme Corp", body.GetProperty("companyName").GetString());
        Assert.Equal(0, body.GetProperty("status").GetInt32()); // Saved = 0
    }

    [Fact]
    public async Task GetJobApplications_ShouldReturnStatusAsInt()
    {
        var (client, userId) = await CreateAuthenticatedClientAsync();

        // Create one application
        await client.PostAsJsonAsync("/api/jobapplications", new
        {
            UserId = userId, Title = "Dev", CompanyName = "Corp"
        });

        // Fetch list
        var response = await client.GetAsync($"/api/jobapplications/{userId}");
        var apps = await response.Content.ReadFromJsonAsync<JsonElement[]>();

        // Status MUST be int (not "Applied" string) for frontend filter to work
        Assert.IsType<int>(
            JsonSerializer.Deserialize<int>(
                apps![0].GetProperty("status").GetRawText()));
    }

    [Fact]
    public async Task UpdateStatus_WithValidId_ShouldReturn204()
    {
        var (client, userId) = await CreateAuthenticatedClientAsync();

        var created = await (await client.PostAsJsonAsync("/api/jobapplications", new
        {
            UserId = userId, Title = "Dev", CompanyName = "Corp"
        })).Content.ReadFromJsonAsync<JsonElement>();

        var id = created.GetProperty("id").GetString();

        var response = await client.PutAsJsonAsync(
            $"/api/jobapplications/{id}",
            new { Status = 1, Notes = "Applied via LinkedIn" }
        );

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}
```

### CustomWebApplicationFactory

```csharp
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");  // ← triggers migration guard skip in Program.cs

        builder.ConfigureServices(services =>
        {
            // Remove real DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (descriptor != null) services.Remove(descriptor);

            // Add InMemory database (per-factory = isolated between test classes)
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase($"TestDb-{Guid.NewGuid()}"));
        });
    }
}
```

---

## 9. CI/CD PIPELINE — GITHUB ACTIONS

### ci.yml Full Structure

```yaml
name: CI/CD Pipeline

on:
  push:
    branches: [ main ]
  pull_request:
    branches: [ main ]

jobs:
  build-and-test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.x'
      - run: dotnet restore
      - run: dotnet build --no-restore --configuration Release
      - run: dotnet test --no-build --configuration Release --verbosity normal
        # Runs: 10 unit tests + 12 integration tests = 22 total

  docker-build:
    needs: build-and-test         # ← only runs if tests pass
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - run: docker build . -t jobtrackerpro:latest
        # Validates Dockerfile is correct, but doesn't push anywhere

  deploy-api:
    needs: docker-build           # ← sequential dependency
    runs-on: ubuntu-latest
    if: github.ref == 'refs/heads/main'   # ← only on main push, not PRs
    steps:
      - uses: actions/checkout@v4
      - run: dotnet publish src/JobTrackerPro.Api -c Release -o ./publish
      - uses: azure/webapps-deploy@v3
        with:
          app-name: ${{ secrets.AZURE_APP_NAME }}
          publish-profile: ${{ secrets.AZURE_PUBLISH_PROFILE }}
          package: './publish'

  deploy-frontend:
    needs: build-and-test         # ← independent from docker-build
    runs-on: ubuntu-latest
    if: github.ref == 'refs/heads/main'
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-node@v4
        with:
          node-version: '22'
      - run: cd frontend && npm ci
      - run: cd frontend && npm run build
        env:
          VITE_API_URL: ${{ secrets.VITE_API_URL }}   # ← injected at build time
      - uses: nwtgck/actions-netlify@v3
        with:
          publish-dir: './frontend/dist'
          production-deploy: true
        env:
          NETLIFY_AUTH_TOKEN: ${{ secrets.NETLIFY_AUTH_TOKEN }}
          NETLIFY_SITE_ID: ${{ secrets.NETLIFY_SITE_ID }}
```

### Docker Multi-Stage Build

```dockerfile
# Stage 1: Build (includes SDK, ~800MB)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/JobTrackerPro.Api/JobTrackerPro.Api.csproj", "src/JobTrackerPro.Api/"]
COPY ["src/JobTrackerPro.Application/...", "..."]
COPY ["src/JobTrackerPro.Infrastructure/...", "..."]
COPY ["src/JobTrackerPro.Domain/...", "..."]
RUN dotnet restore "src/JobTrackerPro.Api/JobTrackerPro.Api.csproj"
COPY . .
RUN dotnet publish "src/JobTrackerPro.Api/JobTrackerPro.Api.csproj" \
    -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime (only runtime, ~220MB → final image ~106MB with trim)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
# Run as non-root user (security best practice)
RUN adduser --disabled-password --gecos '' appuser && chown -R appuser /app
USER appuser
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "JobTrackerPro.Api.dll"]
```

---

## 10. DATABASE SCHEMA

### JobApplications table (PostgreSQL)

```sql
CREATE TABLE "JobApplications" (
    "Id"                      uuid NOT NULL,
    "Title"                   character varying(200) NOT NULL,
    "Description"             text,
    "JobUrl"                  character varying(500),
    "CompanyId"               uuid NOT NULL,
    "Status"                  integer NOT NULL DEFAULT 0,
    "SeniorityLevel"          integer NOT NULL DEFAULT 0,
    "WorkModality"            integer NOT NULL DEFAULT 0,
    "TechStack_Backend"       character varying(500) NOT NULL DEFAULT '',
    "TechStack_Frontend"      character varying(500) NOT NULL DEFAULT '',
    "TechStack_Databases"     character varying(500) NOT NULL DEFAULT '',
    "TechStack_CloudAndDevOps" character varying(500) NOT NULL DEFAULT '',
    "TechStack_Testing"       character varying(500) NOT NULL DEFAULT '',
    "SalaryMin"               numeric,
    "SalaryMax"               numeric,
    "SalaryCurrency"          character varying(10),
    "ContactName"             text,
    "ContactEmail"            text,
    "Source"                  character varying(100),
    "CreatedAt"               timestamp with time zone NOT NULL,
    "AppliedAt"               timestamp with time zone,
    "UpdatedAt"               timestamp with time zone,
    "Notes"                   text,
    "UserId"                  uuid NOT NULL,
    CONSTRAINT "PK_JobApplications" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_JobApplications_Companies" FOREIGN KEY ("CompanyId")
        REFERENCES "Companies" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_JobApplications_Users" FOREIGN KEY ("UserId")
        REFERENCES "Users" ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_JobApplications_UserId" ON "JobApplications" ("UserId");
```

### EF Core Migration Example

```csharp
// How a migration is generated and what it looks like
// Command: dotnet ef migrations add AddJobDescription
// --project src/JobTrackerPro.Infrastructure
// --startup-project src/JobTrackerPro.Api

public partial class AddJobDescription : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Example: adding a new column
        migrationBuilder.AddColumn<string>(
            name: "Description",
            table: "JobApplications",
            type: "text",
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Always implement Down for rollback capability
        migrationBuilder.DropColumn(
            name: "Description",
            table: "JobApplications");
    }
}
```

---

## 11. CONFIGURATION & ENVIRONMENT VARIABLES

### appsettings.json structure

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=...;Database=neondb;Username=...;Password=...;SSL Mode=Require;"
  },
  "JwtSettings": {
    "Secret": "your-256-bit-secret-minimum-32-characters",
    "Issuer": "JobTrackerPro",
    "Audience": "JobTrackerPro",
    "ExpiryMinutes": 60
  },
  "EmailSettings": {
    "Enabled": false,
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "Username": "your@gmail.com",
    "Password": "app-specific-password",
    "FromEmail": "noreply@jobtracker.dev",
    "FromName": "JobTracker Pro"
  },
  "StaleDays": 7,
  "Redis": {
    "ConnectionString": "localhost:6379"
  }
}
```

### Azure App Service Environment Variables

```
# Double-underscore = nested JSON key separator in Azure
JwtSettings__Secret        = <strong-random-secret>
JwtSettings__Issuer        = JobTrackerPro
JwtSettings__Audience      = JobTrackerPro

# Connection String (type: Custom in Azure portal)
DefaultConnection          = Host=...;Database=neondb;Username=...;Password=...;SSL Mode=Require;
```

### Frontend .env files (Vite)

```bash
# frontend/.env.development (local dev, NOT committed)
VITE_API_URL=http://localhost:5000

# frontend/.env.production (injected by CI/CD via GitHub Secret)
VITE_API_URL=https://jobtracker-api-prod-ehg6euckd4evaabw.centralus-01.azurewebsites.net
```

```typescript
// How Vite exposes them at runtime
const API_URL = import.meta.env.VITE_API_URL ?? 'http://localhost:5000';
// import.meta.env = Vite's equivalent of process.env (Node.js)
// Only vars prefixed VITE_ are exposed to the browser bundle
```

---

## 12. DEPENDENCY INJECTION WIRING

```csharp
// Infrastructure/DependencyInjection.cs
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration config)
    {
        // Database
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(config.GetConnectionString("DefaultConnection")));

        // Repositories (Scoped = new instance per HTTP request)
        services.AddScoped<IJobApplicationRepository, JobApplicationRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICompanyRepository, CompanyRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Authentication
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.Configure<JwtSettings>(config.GetSection("JwtSettings"));

        // JWT Bearer validation
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = config["JwtSettings:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = config["JwtSettings:Audience"],
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(config["JwtSettings:Secret"]!)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero  // ← no extra buffer after expiry
                };
            });

        // Redis (with conditional registration)
        var redisConn = config["Redis:ConnectionString"];
        if (!string.IsNullOrEmpty(redisConn))
            services.AddStackExchangeRedisCache(o => o.Configuration = redisConn);
        else
            services.AddDistributedMemoryCache();  // Fallback: in-memory cache
        services.AddScoped<ICacheService, RedisCacheService>();

        // Email
        services.Configure<EmailSettings>(config.GetSection("EmailSettings"));
        services.AddScoped<IEmailService, SmtpEmailService>();

        // Background Service (Singleton — instantiated once, runs forever)
        services.AddHostedService<StaleNotificationService>();

        return services;
    }
}
```

---

## QUICK REFERENCE — KEY DECISIONS

| Decision | Choice | Why |
|----------|--------|-----|
| Architecture | Clean Architecture | Testability, separation of concerns, maintainability |
| Command bus | MediatR | Decouples controllers from handlers; pipeline behaviors for cross-cutting concerns |
| Validation | FluentValidation as pipeline behavior | Single place for validation, not scattered in handlers |
| ORM | EF Core + Repository pattern | Abstracts data access; Repository allows swapping DB in tests |
| Value Object storage | EF Core Owned Entity | No JOIN needed; columns in same table; no extra complexity |
| Auth | JWT + Refresh Token | Stateless (no DB lookup per request); refresh enables long sessions |
| Token storage | localStorage | Simple for SPA; tradeoff: XSS risk (mitigated by short expiry + refresh rotation) |
| Concurrent 401 handling | Queue pattern in interceptor | Prevents multiple simultaneous refresh calls and token invalidation |
| Cache fallback | try/catch → return default | Redis failure should never break the app; data freshness > availability |
| Test DB | EF Core InMemory | Fast, isolated per test class; no Docker needed in CI |
| Frontend state | React Context API | Sufficient for this app size; no Redux needed |
| CSS | Tailwind v4 utility-first | No CSS files to maintain; `dark:` variants built-in |
| Enum serialization | int in DTO | Frontend compares `a.status === filter` (number); string would break it |
| PATCH vs PUT | PATCH=partial edit, PUT=status change | Different domain operations; REST semantics |
| Migrations in prod | `db.Database.Migrate()` on startup | Zero-downtime; no manual steps needed after deploy |
