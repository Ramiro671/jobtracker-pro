namespace JobTrackerPro.Application.Auth.DTOs;

/// <summary>Response returned after successful authentication.</summary>
public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt
);
