namespace Orbit.Application.DTOs;

public record PostAuthorResponse(
    Guid ProfileId,
    string Username,
    string DisplayName,
    string? AvatarUrl
);
