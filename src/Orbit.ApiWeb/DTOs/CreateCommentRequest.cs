namespace Orbit.ApiWeb.DTOs;

public record CreateCommentRequest(
    string Content,
    Guid? ParentCommentId = null
);
