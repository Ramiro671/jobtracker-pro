using JobTrackerPro.Domain.Entities;

namespace JobTrackerPro.Application.Common.Interfaces;

/// <summary>Defines the contract for generating JWT access tokens.</summary>
public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
}
