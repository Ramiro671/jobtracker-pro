using MediatR;

namespace JobTrackerPro.Application.JobApplications.Commands;

/// <summary>Command to create a new job application.</summary>
public record CreateJobApplicationCommand(
    Guid UserId,
    string Title,
    string CompanyName,
    string? JobUrl = null,
    string? Description = null,
    string? Source = null
) : IRequest<Guid>;