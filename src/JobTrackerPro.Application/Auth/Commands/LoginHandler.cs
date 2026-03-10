using JobTrackerPro.Application.Authentication;
using JobTrackerPro.Domain.Interfaces;
using MediatR;

namespace JobTrackerPro.Application.Auth.Commands;

/// <summary>Handles user login and returns a JWT token.</summary>
public class LoginHandler : IRequestHandler<LoginCommand, string>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public LoginHandler(
        IUserRepository userRepository,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _userRepository = userRepository;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<string> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository
            .GetByEmailAsync(request.Email, cancellationToken);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password.");

        return _jwtTokenGenerator.GenerateToken(user);
    }
}
