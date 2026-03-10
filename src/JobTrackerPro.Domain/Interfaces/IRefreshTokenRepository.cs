using JobTrackerPro.Domain.Entities;

namespace JobTrackerPro.Domain.Interfaces;

/// <summary>Repository contract for refresh token operations.</summary>
public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
}
