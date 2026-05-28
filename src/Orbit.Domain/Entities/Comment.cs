using Orbit.Domain.Common;

namespace Orbit.Domain.Entities;

public class Comment : BaseEntity, ISoftDeletable
{
    public Guid ProfileId { get; set; }
    public Guid PostId { get; set; }
    public string Content { get; set; } = null!;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Profile Profile { get; set; } = null!;
    public Post Post { get; set; } = null!;
}
