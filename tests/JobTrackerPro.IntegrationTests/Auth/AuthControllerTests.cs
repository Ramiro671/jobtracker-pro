using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using JobTrackerPro.Application.Auth.Commands;
using JobTrackerPro.Application.Auth.DTOs;

namespace JobTrackerPro.IntegrationTests.Auth;

[Collection("Integration")]
public sealed class AuthControllerTests : BaseIntegrationTest
{
    public AuthControllerTests(CustomWebApplicationFactory factory)
        : base(factory) { }

    [Fact]
    public async Task Register_WithValidData_ShouldReturn200WithTokens()
    {
        // Arrange
        var command = new RegisterCommand(
            "Integration Test User",
            $"test_{Guid.NewGuid()}@jobtracker.com",
            "Password123!");

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/register", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        auth.Should().NotBeNull();
        auth!.AccessToken.Should().NotBeNullOrEmpty();
        auth.RefreshToken.Should().NotBeNullOrEmpty();
        auth.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ShouldReturn500()
    {
        // Arrange
        var email = $"duplicate_{Guid.NewGuid()}@jobtracker.com";
        var command = new RegisterCommand("User", email, "Password123!");

        // Register first time
        await Client.PostAsJsonAsync("/api/auth/register", command);

        // Act — register again with same email
        var response = await Client.PostAsJsonAsync("/api/auth/register", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturn200WithTokens()
    {
        // Arrange — register first
        var email = $"login_{Guid.NewGuid()}@jobtracker.com";
        var password = "Password123!";
        await Client.PostAsJsonAsync("/api/auth/register",
            new RegisterCommand("Test", email, password));

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/login",
            new LoginCommand(email, password));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        auth!.AccessToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_WithWrongPassword_ShouldReturn401()
    {
        // Arrange — register first
        var email = $"wrong_{Guid.NewGuid()}@jobtracker.com";
        await Client.PostAsJsonAsync("/api/auth/register",
            new RegisterCommand("Test", email, "Password123!"));

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/login",
            new LoginCommand(email, "WrongPassword!"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
