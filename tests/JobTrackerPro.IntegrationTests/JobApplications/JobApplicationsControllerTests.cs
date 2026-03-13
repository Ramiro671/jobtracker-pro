using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using JobTrackerPro.Application.DTOs;
using JobTrackerPro.Application.JobApplications.Commands;
using JobTrackerPro.Domain.Enums;

namespace JobTrackerPro.IntegrationTests.JobApplications;

[Collection("Integration")]
public sealed class JobApplicationsControllerTests : BaseIntegrationTest
{
    public JobApplicationsControllerTests(CustomWebApplicationFactory factory)
        : base(factory) { }

    // ── GET ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetJobApplications_WithoutToken_ShouldReturn401()
    {
        var response = await Client.GetAsync($"/api/jobapplications/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetJobApplications_AfterCreating_ShouldReturnApplications()
    {
        var userId = Guid.NewGuid();
        var token = await GetAuthTokenAsync($"get_{Guid.NewGuid()}@test.com");
        SetBearerToken(token);

        await Client.PostAsJsonAsync("/api/jobapplications",
            new CreateJobApplicationCommand(userId, "Dev", "Google", null, null, "Direct"));

        var response = await Client.GetAsync($"/api/jobapplications/{userId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var apps = await response.Content.ReadFromJsonAsync<List<JobApplicationDto>>();
        apps.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetJobApplications_ShouldReturnStatusAsInt()
    {
        var userId = Guid.NewGuid();
        var token = await GetAuthTokenAsync($"statusint_{Guid.NewGuid()}@test.com");
        SetBearerToken(token);

        await Client.PostAsJsonAsync("/api/jobapplications",
            new CreateJobApplicationCommand(userId, "Dev", "Acme", null, null, "Direct"));

        var response = await Client.GetAsync($"/api/jobapplications/{userId}");
        var apps = await response.Content.ReadFromJsonAsync<List<JobApplicationDto>>();

        apps.Should().HaveCount(1);
        apps![0].Status.Should().Be((int)ApplicationStatus.Saved);
    }

    // ── CREATE ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateJobApplication_WithValidToken_ShouldReturn201()
    {
        var token = await GetAuthTokenAsync($"create_{Guid.NewGuid()}@test.com");
        SetBearerToken(token);

        var command = new CreateJobApplicationCommand(
            UserId: Guid.NewGuid(),
            Title: "Senior .NET Developer",
            CompanyName: "Anthropic",
            JobUrl: "https://anthropic.com/jobs/1",
            Description: "Remote position",
            Source: "LinkedIn");

        var response = await Client.PostAsJsonAsync("/api/jobapplications", command);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<Dictionary<string, Guid>>();
        body!["id"].Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task CreateJobApplication_WithInvalidData_ShouldReturn400()
    {
        var token = await GetAuthTokenAsync($"invalid_{Guid.NewGuid()}@test.com");
        SetBearerToken(token);

        var command = new CreateJobApplicationCommand(
            UserId: Guid.NewGuid(),
            Title: "",
            CompanyName: "",
            JobUrl: null,
            Description: null,
            Source: "");

        var response = await Client.PostAsJsonAsync("/api/jobapplications", command);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateJobApplication_WithoutToken_ShouldReturn401()
    {
        var command = new CreateJobApplicationCommand(
            Guid.NewGuid(), "Dev", "Acme", null, null, "Direct");

        var response = await Client.PostAsJsonAsync("/api/jobapplications", command);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── UPDATE STATUS ────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateStatus_WithValidId_ShouldReturn204()
    {
        var token = await GetAuthTokenAsync($"update_{Guid.NewGuid()}@test.com");
        SetBearerToken(token);

        var createResponse = await Client.PostAsJsonAsync("/api/jobapplications",
            new CreateJobApplicationCommand(Guid.NewGuid(), "Dev", "Meta", null, null, "Direct"));
        var created = await createResponse.Content.ReadFromJsonAsync<Dictionary<string, Guid>>();
        var appId = created!["id"];

        var response = await Client.PutAsJsonAsync(
            $"/api/jobapplications/{appId}",
            new { newStatus = (int)ApplicationStatus.Applied, notes = "Applied online" });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task UpdateStatus_WithUnknownId_ShouldReturn404()
    {
        var token = await GetAuthTokenAsync($"upd404_{Guid.NewGuid()}@test.com");
        SetBearerToken(token);

        var response = await Client.PutAsJsonAsync(
            $"/api/jobapplications/{Guid.NewGuid()}",
            new { newStatus = (int)ApplicationStatus.Applied, notes = (string?)null });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── EDIT ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Edit_WithValidId_ShouldReturn204AndPersistChanges()
    {
        var userId = Guid.NewGuid();
        var token = await GetAuthTokenAsync($"edit_{Guid.NewGuid()}@test.com");
        SetBearerToken(token);

        var createResponse = await Client.PostAsJsonAsync("/api/jobapplications",
            new CreateJobApplicationCommand(userId, "Old Title", "OldCo", null, null, "Direct"));
        var created = await createResponse.Content.ReadFromJsonAsync<Dictionary<string, Guid>>();
        var appId = created!["id"];

        var editResponse = await Client.PatchAsJsonAsync(
            $"/api/jobapplications/{appId}",
            new { title = "New Title", jobUrl = "https://example.com", notes = "Updated notes" });

        editResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var apps = await (await Client.GetAsync($"/api/jobapplications/{userId}"))
            .Content.ReadFromJsonAsync<List<JobApplicationDto>>();
        apps![0].Title.Should().Be("New Title");
        apps[0].Notes.Should().Be("Updated notes");
    }

    [Fact]
    public async Task Edit_WithUnknownId_ShouldReturn404()
    {
        var token = await GetAuthTokenAsync($"edit404_{Guid.NewGuid()}@test.com");
        SetBearerToken(token);

        var response = await Client.PatchAsJsonAsync(
            $"/api/jobapplications/{Guid.NewGuid()}",
            new { title = "Test", jobUrl = (string?)null, notes = (string?)null });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── DELETE ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task Delete_WithValidId_ShouldReturn204AndRemoveApplication()
    {
        var userId = Guid.NewGuid();
        var token = await GetAuthTokenAsync($"del_{Guid.NewGuid()}@test.com");
        SetBearerToken(token);

        var createResponse = await Client.PostAsJsonAsync("/api/jobapplications",
            new CreateJobApplicationCommand(userId, "To Delete", "Acme", null, null, "Direct"));
        var created = await createResponse.Content.ReadFromJsonAsync<Dictionary<string, Guid>>();
        var appId = created!["id"];

        var deleteResponse = await Client.DeleteAsync($"/api/jobapplications/{appId}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var apps = await (await Client.GetAsync($"/api/jobapplications/{userId}"))
            .Content.ReadFromJsonAsync<List<JobApplicationDto>>();
        apps.Should().BeEmpty();
    }

    [Fact]
    public async Task Delete_WithUnknownId_ShouldReturn404()
    {
        var token = await GetAuthTokenAsync($"del404_{Guid.NewGuid()}@test.com");
        SetBearerToken(token);

        var response = await Client.DeleteAsync($"/api/jobapplications/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
