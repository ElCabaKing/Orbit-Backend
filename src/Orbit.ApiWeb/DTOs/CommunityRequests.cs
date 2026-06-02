namespace Orbit.ApiWeb.DTOs;

public record CreateCommunityRequest(
    string Name,
    string? Description,
    bool IsPrivate = false
);

public record UpdateCommunityRequest(
    string? Name,
    string? Description,
    bool? IsPrivate
);

public record ModeratorRequest(string Username);
