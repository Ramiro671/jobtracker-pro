using JobTrackerPro.Application.Auth.DTOs;
using JobTrackerPro.Application.Common.Interfaces;
using JobTrackerPro.Domain.Entities;
using JobTrackerPro.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace JobTrackerPro.Application.Auth.Commands;

/// <summary>Handles user registration and returns a JWT token and refresh token.</summary>
public class RegisterHandler : IRequestHandler<RegisterCommand, AuthResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly ILogger<RegisterHandler> _logger;

    public RegisterHandler(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IUnitOfWork unitOfWork,
        IJwtTokenGenerator jwtTokenGenerator,
        ILogger<RegisterHandler> logger)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _unitOfWork = unitOfWork;
        _jwtTokenGenerator = jwtTokenGenerator;
        _logger = logger;
    }

    public async Task<AuthResponse> Handle(
        RegisterCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Registering new user with email {Email}", request.Email);

        if (await _userRepository.ExistsAsync(request.Email, cancellationToken))
        {
            _logger.LogWarning("Registration failed — email {Email} already exists", request.Email);
            throw new InvalidOperationException($"Email '{request.Email}' is already registered.");
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        var user = User.Create(request.FullName, request.Email, passwordHash);

        await _userRepository.AddAsync(user, cancellationToken);

        var refreshToken = RefreshToken.Create(user.Id);
        await _refreshTokenRepository.AddAsync(refreshToken, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var accessToken = _jwtTokenGenerator.GenerateToken(user);

        _logger.LogInformation("User {UserId} registered successfully", user.Id);

        return new AuthResponse(
            accessToken,
            refreshToken.Token,
            DateTime.UtcNow.AddMinutes(60));
    }
}
