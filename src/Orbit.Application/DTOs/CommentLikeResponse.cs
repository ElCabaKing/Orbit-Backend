namespace Orbit.Application.DTOs;

public record CommentLikeResponse(
    Guid CommentId,
    bool IsLiked,
    int LikeCount
);
