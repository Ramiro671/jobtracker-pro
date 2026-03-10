using MediatR;

namespace JobTrackerPro.Application.JobApplications.Commands;

/// <summary>Deletes a job application by its identifier.</summary>
public record DeleteJobApplicationCommand(Guid Id) : IRequest<bool>;