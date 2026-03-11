using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using JobTrackerPro.Application.JobApplications.Commands;

namespace JobTrackerPro.IntegrationTests.JobApplications;

[Collection("Integration")]
public sealed class JobApplicationsControllerTests : BaseIntegrationTest
{
    public JobApplicationsControllerTests(CustomWebApplicationFactory factory)
        : base(factory) { }

    [Fact]
    public async Task GetJobApplications_WithoutToken_ShouldReturn401()
    {
        // Act
        var response = await Client.GetAsync(
            $"/api/jobapplications/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateJobApplication_WithValidToken_ShouldReturn201()
    {
        // Arrange
        var token = await GetAuthTokenAsync($"create_{Guid.NewGuid()}@test.com");
        SetBearerToken(token);

        var command = new CreateJobApplicationCommand(
            UserId: Guid.NewGuid(),
            Title: "Senior .NET Developer",
            CompanyName: "Anthropic",
            JobUrl: "https://anthropic.com/jobs/1",
            Description: "Remote position",
            Source: "LinkedIn");

        // Act
        var response = await Client.PostAsJsonAsync("/api/jobapplications", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadFromJsonAsync<Dictionary<string, Guid>>();
        body!["id"].Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task CreateJobApplication_WithInvalidData_ShouldReturn400()
    {
        // Arrange
        var token = await GetAuthTokenAsync($"invalid_{Guid.NewGuid()}@test.com");
        SetBearerToken(token);

        var command = new CreateJobApplicationCommand(
            UserId: Guid.NewGuid(),
            Title: "",
            CompanyName: "",
            JobUrl: null,
            Description: null,
            Source: "");

        // Act
        var response = await Client.PostAsJsonAsync("/api/jobapplications", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetJobApplications_AfterCreating_ShouldReturnApplications()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var token = await GetAuthTokenAsync($"get_{Guid.NewGuid()}@test.com");
        SetBearerToken(token);

        // Create an application
        await Client.PostAsJsonAsync("/api/jobapplications",
            new CreateJobApplicationCommand(
                userId, "Dev", "Google",
                null, null, "Direct"));

        // Act
        var response = await Client.GetAsync($"/api/jobapplications/{userId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var apps = await response.Content
            .ReadFromJsonAsync<List<Dictionary<string, object>>>();
        apps.Should().HaveCount(1);
    }
}
