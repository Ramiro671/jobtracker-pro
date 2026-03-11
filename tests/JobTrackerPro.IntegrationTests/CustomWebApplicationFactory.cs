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
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the PostgreSQL DbContextOptions registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

            if (descriptor != null)
                services.Remove(descriptor);

            // Create an isolated service provider for InMemory to avoid
            // "two providers" conflict (Npgsql services remain in the main container)
            var dbServiceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            // Re-register DbContext with InMemory using the isolated provider
            services.AddDbContext<ApplicationDbContext>((_, options) =>
                options.UseInMemoryDatabase("JobTrackerTestDb")
                       .UseInternalServiceProvider(dbServiceProvider));
        });

        builder.UseEnvironment("Testing");
    }
}
