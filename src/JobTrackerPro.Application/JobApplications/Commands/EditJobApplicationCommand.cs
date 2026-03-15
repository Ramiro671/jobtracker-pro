using MediatR;

namespace JobTrackerPro.Application.JobApplications.Commands;

/// <summary>Edits the title, company, job URL, and notes of an existing job application.</summary>
public record EditJobApplicationCommand(
    Guid Id,
    string Title,
    string? CompanyName,
    string? JobUrl,
    string? Notes
) : IRequest<bool>;
