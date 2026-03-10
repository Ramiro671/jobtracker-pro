using JobTrackerPro.Application.Auth.DTOs;
using MediatR;

namespace JobTrackerPro.Application.Auth.Commands;

/// <summary>Command to authenticate an existing user.</summary>
public record LoginCommand(
    string Email,
    string Password
) : IRequest<AuthResponse>;
