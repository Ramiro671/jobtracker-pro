using JobTrackerPro.Application.Auth.DTOs;
using MediatR;

namespace JobTrackerPro.Application.Auth.Commands;

/// <summary>Command to register a new user.</summary>
public record RegisterCommand(
    string FullName,
    string Email,
    string Password
) : IRequest<AuthResponse>;
