using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Orbit.Domain.Entities;

namespace Orbit.Infrastructure.Configurations;

public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("user_roles");

        builder.HasKey(ur => new { ur.ProfileId, ur.RoleId });

        builder.Property(ur => ur.ProfileId)
            .HasColumnName("profile_id")
            .IsRequired();

        builder.Property(ur => ur.RoleId)
            .HasColumnName("role_id")
            .IsRequired();

        builder.Property(ur => ur.AssignedAt)
            .HasColumnName("assigned_at")
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasOne(ur => ur.Profile)
            .WithMany(p => p.UserRoles)
            .HasForeignKey(ur => ur.ProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ur => ur.Role)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(ur => ur.RoleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
