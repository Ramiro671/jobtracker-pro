using System.Net.Http.Json;
using JobTrackerPro.Application.Auth.Commands;
using JobTrackerPro.Application.Auth.DTOs;
using JobTrackerPro.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace JobTrackerPro.IntegrationTests;

/// <summary>Base class providing shared setup for all integration tests.</summary>
public abstract class BaseIntegrationTest
{
    protected readonly HttpClient Client;
    protected readonly CustomWebApplicationFactory Factory;

    protected BaseIntegrationTest(CustomWebApplicationFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
    }

    /// <summary>Registers a test user and returns the Bearer token.</summary>
    protected async Task<string> GetAuthTokenAsync(
        string email = "test@jobtracker.com",
        string password = "Password123!")
    {
        var register = new RegisterCommand("Test User", email, password);

        var response = await Client.PostAsJsonAsync("/api/auth/register", register);

        // If already registered, login instead
        if (!response.IsSuccessStatusCode)
        {
            var login = new LoginCommand(email, password);
            response = await Client.PostAsJsonAsync("/api/auth/login", login);
        }

        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        return auth!.AccessToken;
    }

    /// <summary>Sets the Authorization header for authenticated requests.</summary>
    protected void SetBearerToken(string token)
    {
        Client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    /// <summary>Resets the in-memory database between tests.</summary>
    protected void ResetDatabase()
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
    }
}
