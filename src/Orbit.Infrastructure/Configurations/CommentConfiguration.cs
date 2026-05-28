using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Orbit.Domain.Entities;

namespace Orbit.Infrastructure.Configurations;

public class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.ToTable("comments");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.ProfileId)
            .HasColumnName("profile_id")
            .IsRequired();

        builder.Property(c => c.PostId)
            .HasColumnName("post_id")
            .IsRequired();

        builder.Property(c => c.Content)
            .HasColumnName("content")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(c => c.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.Property(c => c.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasOne(c => c.Profile)
            .WithMany()
            .HasForeignKey(c => c.ProfileId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(c => c.Post)
            .WithMany(p => p.Comments)
            .HasForeignKey(c => c.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(c => c.PostId)
            .HasDatabaseName("ix_comments_post_id");

        builder.HasIndex(c => c.ProfileId)
            .HasDatabaseName("ix_comments_profile_id");

        builder.HasQueryFilter(c => c.IsActive && c.Post.IsActive);
    }
}
