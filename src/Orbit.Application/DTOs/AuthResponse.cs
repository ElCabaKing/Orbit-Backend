namespace Orbit.Application.DTOs;

public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    ProfileResponse Profile
);
