using JobTrackerPro.Application.DTOs;
using MediatR;

namespace JobTrackerPro.Application.JobApplications.Queries;

/// <summary>Query to retrieve all job applications for a given user.</summary>
public record GetJobApplicationsQuery(Guid UserId) : IRequest<IReadOnlyList<JobApplicationDto>>;