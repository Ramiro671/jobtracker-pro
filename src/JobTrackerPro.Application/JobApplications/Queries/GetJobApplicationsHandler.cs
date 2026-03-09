using JobTrackerPro.Application.DTOs;
using JobTrackerPro.Domain.Interfaces;
using MediatR;

namespace JobTrackerPro.Application.JobApplications.Queries;

/// <summary>Handles retrieval of all job applications for a user.</summary>
public class GetJobApplicationsHandler : IRequestHandler<GetJobApplicationsQuery, IReadOnlyList<JobApplicationDto>>
{
    private readonly IJobApplicationRepository _repository;

    public GetJobApplicationsHandler(IJobApplicationRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<JobApplicationDto>> Handle(
        GetJobApplicationsQuery query,
        CancellationToken cancellationToken)
    {
        var applications = await _repository.GetAllByUserIdAsync(query.UserId, cancellationToken);

        return applications.Select(a => new JobApplicationDto(
            Id: a.Id,
            Title: a.Title,
            CompanyName: a.Company?.Name ?? string.Empty,
            Status: a.Status.ToString(),
            WorkModality: a.WorkModality.ToString(),
            SeniorityLevel: a.SeniorityLevel.ToString(),
            JobUrl: a.JobUrl,
            Source: a.Source,
            SalaryMin: a.SalaryMin,
            SalaryMax: a.SalaryMax,
            SalaryCurrency: a.SalaryCurrency,
            Notes: a.Notes,
            CreatedAt: a.CreatedAt,
            AppliedAt: a.AppliedAt
        )).ToList();
    }
}