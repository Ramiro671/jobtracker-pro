namespace JobTrackerPro.Domain.Interfaces;

/// <summary>
/// Abstraction for committing multiple repository changes as a single transaction.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}