namespace Orbit.Application.DTOs;

public record LikeResponse(
    Guid PostId,
    bool IsLiked,
    int LikeCount
);
