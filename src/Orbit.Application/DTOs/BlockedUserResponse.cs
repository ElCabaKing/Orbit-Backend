namespace Orbit.Application.DTOs;

public record BlockedUserResponse(
    Guid ProfileId,
    string Username,
    string DisplayName,
    string? AvatarUrl,
    DateTime BlockedAt
);
