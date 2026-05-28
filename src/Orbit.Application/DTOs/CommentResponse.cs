namespace Orbit.Application.DTOs;

public record CommentResponse(
    Guid Id,
    PostAuthorResponse Author,
    string Content,
    DateTime CreatedAt
);
