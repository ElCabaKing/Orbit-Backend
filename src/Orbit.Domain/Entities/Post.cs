using Orbit.Domain.Common;

namespace Orbit.Domain.Entities;

public class Post : BaseEntity, ISoftDeletable
{
    public Guid ProfileId { get; set; }
    public string Content { get; set; } = null!;
    public string? MediaUrl { get; set; }
    public string? MediaPublicId { get; set; }
    public string? MediaType { get; set; }
    public int LikeCount { get; set; }
    public int CommentCount { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Profile Profile { get; set; } = null!;
    public ICollection<PostLike> PostLikes { get; set; } = [];
    public ICollection<Comment> Comments { get; set; } = [];
}
