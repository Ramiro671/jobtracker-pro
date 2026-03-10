using JobTrackerPro.Application.Auth.DTOs;
using MediatR;

namespace JobTrackerPro.Application.Auth.Commands;

public record RefreshTokenCommand(string Token) : IRequest<AuthResponse>;
