using JobTrackerPro.Domain.Entities;

namespace JobTrackerPro.Application.Authentication;

/// <summary>Generates JWT tokens for authenticated users.</summary>
public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
}
