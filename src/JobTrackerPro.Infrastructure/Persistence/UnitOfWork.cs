using JobTrackerPro.Domain.Interfaces;

namespace JobTrackerPro.Infrastructure.Persistence;

/// <summary>
/// Coordinates multiple repository operations in a single database transaction.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);
}