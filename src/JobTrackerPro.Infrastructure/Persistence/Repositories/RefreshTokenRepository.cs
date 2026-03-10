using JobTrackerPro.Domain.Entities;
using JobTrackerPro.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JobTrackerPro.Infrastructure.Persistence.Repositories;

/// <summary>EF Core implementation of IRefreshTokenRepository.</summary>
public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly ApplicationDbContext _context;

    public RefreshTokenRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<RefreshToken?> GetByTokenAsync(
        string token,
        CancellationToken cancellationToken = default)
        => await _context.RefreshTokens
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Token == token, cancellationToken);

    public async Task AddAsync(
        RefreshToken refreshToken,
        CancellationToken cancellationToken = default)
        => await _context.RefreshTokens.AddAsync(refreshToken, cancellationToken);
}
