namespace Orbit.Application.DTOs;

public record PostResponse(
    Guid Id,
    PostAuthorResponse Author,
    string Content,
    string? MediaUrl,
    string? MediaType,
    int LikeCount,
    int CommentCount,
    bool IsLiked,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
