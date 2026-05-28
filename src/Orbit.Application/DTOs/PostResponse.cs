namespace Orbit.Application.DTOs;

public record PostResponse(
    Guid Id,
    PostAuthorResponse Author,
    string Content,
    List<PostMediaResponse> Media,
    int LikeCount,
    int CommentCount,
    bool IsLiked,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
