using JobTrackerPro.Application.Auth.DTOs;
using MediatR;

namespace JobTrackerPro.Application.Auth.Commands;

/// <summary>Command to rotate a refresh token and obtain a new access token.</summary>
public record RefreshTokenCommand(string Token) : IRequest<AuthResponse>;
