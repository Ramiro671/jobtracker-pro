using MediatR;

namespace JobTrackerPro.Application.JobApplications.Commands;

/// <summary>Edits the title, job URL, and notes of an existing job application.</summary>
public record EditJobApplicationCommand(
    Guid Id,
    string Title,
    string? JobUrl,
    string? Notes
) : IRequest<bool>;
