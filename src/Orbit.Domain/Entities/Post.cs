using Orbit.Domain.Common;

namespace Orbit.Domain.Entities;

public class Post : BaseEntity, ISoftDeletable
{
    public Guid ProfileId { get; set; }
    public string Content { get; set; } = null!;
    public int LikeCount { get; set; }
    public int CommentCount { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Profile Profile { get; set; } = null!;
    public ICollection<PostMedia> PostMedia { get; set; } = [];
    public ICollection<PostLike> PostLikes { get; set; } = [];
    public ICollection<Comment> Comments { get; set; } = [];
}
