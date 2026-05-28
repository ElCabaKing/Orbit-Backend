namespace Orbit.ApiWeb.DTOs;

public record UpdateProfileRequest(
    string? DisplayName,
    string? Bio,
    bool? IsPrivate
);
