using JobTrackerPro.Domain.Entities;
using JobTrackerPro.Domain.Enums;

namespace JobTrackerPro.Domain.Interfaces
{
    /// <summary>
    /// Repository interface for JobApplication aggregate root.
    /// Implementation will be in Infrastructure layer (EF Core + PostgreSQL).
    /// </summary>
    public interface IJobApplicationRepository
    {
        Task<JobApplication?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<JobApplication>> GetAllByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<JobApplication>> GetByStatusAsync(Guid userId, ApplicationStatus status, CancellationToken cancellationToken = default);
        Task AddAsync(JobApplication application, CancellationToken cancellationToken = default);
        Task UpdateAsync(JobApplication application, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

        void Delete(JobApplication application);
    }
}
