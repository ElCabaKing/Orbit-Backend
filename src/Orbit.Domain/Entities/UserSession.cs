using Orbit.Domain.Common;

namespace Orbit.Domain.Entities;

public class UserSession : BaseEntity
{
    public Guid AuthUserId { get; set; }
    public string RefreshTokenHash { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }

    public AuthUser AuthUser { get; set; } = null!;
}
