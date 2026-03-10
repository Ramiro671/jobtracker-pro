using JobTrackerPro.Application.Authentication;
using JobTrackerPro.Domain.Entities;
using JobTrackerPro.Domain.Interfaces;
using MediatR;

namespace JobTrackerPro.Application.Auth.Commands;

/// <summary>Handles user registration and returns a JWT token.</summary>
public class RegisterHandler : IRequestHandler<RegisterCommand, string>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public RegisterHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<string> Handle(
        RegisterCommand request,
        CancellationToken cancellationToken)
    {
        if (await _userRepository.ExistsAsync(request.Email, cancellationToken))
            throw new InvalidOperationException($"Email '{request.Email}' is already registered.");

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var user = User.Create(request.FullName, request.Email, passwordHash);

        await _userRepository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _jwtTokenGenerator.GenerateToken(user);
    }
}
