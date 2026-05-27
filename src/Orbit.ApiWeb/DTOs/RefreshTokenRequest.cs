namespace Orbit.ApiWeb.DTOs;

public record RefreshTokenRequest(
    string AccessToken,
    string RefreshToken
);
