using Microsoft.Extensions.DependencyInjection;

namespace JobTrackerPro.Application.Common;

/// <summary>Registers Application layer services in the DI container.</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        return services;
    }
}