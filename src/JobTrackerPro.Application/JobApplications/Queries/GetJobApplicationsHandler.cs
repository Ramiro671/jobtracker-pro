using JobTrackerPro.Application.Common.Interfaces;
using JobTrackerPro.Application.DTOs;
using JobTrackerPro.Domain.Interfaces;
using MediatR;

namespace JobTrackerPro.Application.JobApplications.Queries;

/// <summary>Handles retrieval of all job applications for a user.</summary>
public class GetJobApplicationsHandler : IRequestHandler<GetJobApplicationsQuery, IReadOnlyList<JobApplicationDto>>
{
    private readonly IJobApplicationRepository _repository;
    private readonly ICacheService _cache;

    public GetJobApplicationsHandler(
        IJobApplicationRepository repository,
        ICacheService cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public async Task<IReadOnlyList<JobApplicationDto>> Handle(
        GetJobApplicationsQuery query,
        CancellationToken cancellationToken)
    {
        var cacheKey = $"job-applications:{query.UserId}";

        // Try cache first
        var cached = await _cache.GetAsync<List<JobApplicationDto>>(cacheKey, cancellationToken);
        if (cached is not null)
            return cached;

        // Cache miss — query DB
        var applications = await _repository.GetAllByUserIdAsync(query.UserId, cancellationToken);

        var dtos = applications.Select(a => new JobApplicationDto(
            Id: a.Id,
            Title: a.Title,
            CompanyName: a.Company?.Name ?? string.Empty,
            Status: (int)a.Status,
            WorkModality: a.WorkModality.ToString(),
            SeniorityLevel: a.SeniorityLevel.ToString(),
            JobUrl: a.JobUrl,
            Source: a.Source,
            SalaryMin: a.SalaryMin,
            SalaryMax: a.SalaryMax,
            SalaryCurrency: a.SalaryCurrency,
            Notes: a.Notes,
            CreatedAt: a.CreatedAt,
            AppliedAt: a.AppliedAt,
            UpdatedAt: a.UpdatedAt
        )).ToList();

        // Store in cache for 10 minutes
        await _cache.SetAsync(cacheKey, dtos, TimeSpan.FromMinutes(10), cancellationToken);

        return dtos;
    }
}
