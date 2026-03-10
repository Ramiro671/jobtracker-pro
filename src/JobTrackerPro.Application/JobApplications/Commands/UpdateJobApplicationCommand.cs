using JobTrackerPro.Domain.Enums;
using MediatR;

namespace JobTrackerPro.Application.JobApplications.Commands;

/// <summary>Updates the status of an existing job application.</summary>
public record UpdateJobApplicationCommand(
    Guid Id,
    ApplicationStatus NewStatus,
    string? Notes
) : IRequest<bool>;