using JobTrackerPro.Domain.Entities;

namespace JobTrackerPro.Domain.Interfaces
{
    /// <summary>
    /// Repository interface for User entity.
    /// </summary>
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task AddAsync(User user, CancellationToken cancellationToken = default);
        Task UpdateAsync(User user, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(string email, CancellationToken cancellationToken = default);
    }
}
