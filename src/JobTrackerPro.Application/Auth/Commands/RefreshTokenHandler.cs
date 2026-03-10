using JobTrackerPro.Application.Auth.DTOs;
using JobTrackerPro.Application.Common.Interfaces;
using JobTrackerPro.Domain.Entities;
using JobTrackerPro.Domain.Interfaces;
using MediatR;

namespace JobTrackerPro.Application.Auth.Commands;

/// <summary>Handles refresh token rotation and returns a new access token.</summary>
public class RefreshTokenHandler : IRequestHandler<RefreshTokenCommand, AuthResponse>
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IUnitOfWork _unitOfWork;

    public RefreshTokenHandler(
        IRefreshTokenRepository refreshTokenRepository,
        IJwtTokenGenerator jwtTokenGenerator,
        IUnitOfWork unitOfWork)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _jwtTokenGenerator = jwtTokenGenerator;
        _unitOfWork = unitOfWork;
    }

    public async Task<AuthResponse> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        var refreshToken = await _refreshTokenRepository
            .GetByTokenAsync(request.Token, cancellationToken);

        if (refreshToken is null || !refreshToken.IsActive)
            throw new UnauthorizedAccessException("Invalid or expired refresh token.");

        refreshToken.Revoke();

        var newRefreshToken = RefreshToken.Create(refreshToken.UserId);
        await _refreshTokenRepository.AddAsync(newRefreshToken, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var accessToken = _jwtTokenGenerator.GenerateToken(refreshToken.User!);

        return new AuthResponse(
            accessToken,
            newRefreshToken.Token,
            DateTime.UtcNow.AddMinutes(60));
    }
}
