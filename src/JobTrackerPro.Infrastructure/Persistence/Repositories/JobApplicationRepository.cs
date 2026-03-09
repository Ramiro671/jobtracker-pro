using JobTrackerPro.Domain.Entities;
using JobTrackerPro.Domain.Enums;
using JobTrackerPro.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JobTrackerPro.Infrastructure.Persistence.Repositories;

/// <summary>EF Core implementation of IJobApplicationRepository.</summary>
public class JobApplicationRepository : IJobApplicationRepository
{
    private readonly ApplicationDbContext _context;

    public JobApplicationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<JobApplication?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.JobApplications
            .Include(j => j.Company)
            .FirstOrDefaultAsync(j => j.Id == id, cancellationToken);

    public async Task<IReadOnlyList<JobApplication>> GetAllByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        => await _context.JobApplications
            .Include(j => j.Company)
            .Where(j => j.UserId == userId)
            .OrderByDescending(j => j.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<JobApplication>> GetByStatusAsync(Guid userId, ApplicationStatus status, CancellationToken cancellationToken = default)
        => await _context.JobApplications
            .Include(j => j.Company)
            .Where(j => j.UserId == userId && j.Status == status)
            .OrderByDescending(j => j.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(JobApplication application, CancellationToken cancellationToken = default)
        => await _context.JobApplications.AddAsync(application, cancellationToken);

    public async Task UpdateAsync(JobApplication application, CancellationToken cancellationToken = default)
    {
        _context.JobApplications.Update(application);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var application = await GetByIdAsync(id, cancellationToken);
        if (application is not null)
            _context.JobApplications.Remove(application);
    }
}