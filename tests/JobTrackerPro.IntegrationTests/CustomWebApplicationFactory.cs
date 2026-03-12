using JobTrackerPro.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace JobTrackerPro.IntegrationTests;

/// <summary>
/// Custom factory that replaces PostgreSQL with an in-memory database
/// so integration tests run without Docker.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public Task InitializeAsync()
    {
        StartServer();
        return Task.CompletedTask;
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the PostgreSQL DbContextOptions registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

            if (descriptor != null)
                services.Remove(descriptor);

            // Isolated service provider for InMemory — keeps Npgsql services separate
            // to avoid the "two providers for the same DbContext" EF Core conflict.
            var dbServiceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            services.AddDbContext<ApplicationDbContext>((_, options) =>
                options.UseInMemoryDatabase("JobTrackerTestDb")
                       .UseInternalServiceProvider(dbServiceProvider));
        });

        builder.UseEnvironment("Testing");
    }
}
