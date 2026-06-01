namespace Orbit.Application.DTOs;

public record SearchProfileResponse(
    Guid ProfileId,
    string Username,
    string DisplayName,
    string? AvatarUrl,
    bool IsFollowing
);
