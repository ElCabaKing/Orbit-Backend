namespace Orbit.Domain.Entities;

public class UserRole
{
    public Guid ProfileId { get; set; }
    public Guid RoleId { get; set; }
    public DateTime AssignedAt { get; set; }

    public Profile Profile { get; set; } = null!;
    public Role Role { get; set; } = null!;
}
