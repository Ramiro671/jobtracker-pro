using System.Text;
using JobTrackerPro.Application.Authentication;
using JobTrackerPro.Domain.Interfaces;
using JobTrackerPro.Infrastructure.Authentication;
using JobTrackerPro.Infrastructure.Persistence;
using JobTrackerPro.Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace JobTrackerPro.Infrastructure;

/// <summary>Registers Infrastructure layer services in the DI container.</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // EF Core + PostgreSQL
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        // Repositories
        services.AddScoped<IJobApplicationRepository, JobApplicationRepository>();
        services.AddScoped<ICompanyRepository, CompanyRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // JWT Settings
        services.Configure<JwtSettings>(
            configuration.GetSection("JwtSettings"));

        // JWT Token Generator
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

        // JWT Authentication
        var jwtSettings = configuration
            .GetSection("JwtSettings")
            .Get<JwtSettings>()!;

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSettings.Secret))
                };
            });

        return services;
    }
}
