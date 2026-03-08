using JobTrackerPro.Domain.Entities;

namespace JobTrackerPro.Domain.Interfaces
{
    /// <summary>
    /// Repository interface for Company entity.
    /// </summary>
    public interface ICompanyRepository
    {
        Task<Company?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Company?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Company>> GetAllAsync(CancellationToken cancellationToken = default);
        Task AddAsync(Company company, CancellationToken cancellationToken = default);
        Task UpdateAsync(Company company, CancellationToken cancellationToken = default);
    }
}
