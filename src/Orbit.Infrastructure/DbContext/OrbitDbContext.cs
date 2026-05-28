using Microsoft.EntityFrameworkCore;
using Orbit.Domain.Entities;

namespace Orbit.Infrastructure.DbContext;

public class OrbitDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public OrbitDbContext(DbContextOptions<OrbitDbContext> options) : base(options) { }

    public DbSet<AuthUser> AuthUsers => Set<AuthUser>();
    public DbSet<Profile> Profiles => Set<Profile>();
    public DbSet<UserSession> UserSessions => Set<UserSession>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<UserPrefix> UserPrefixes => Set<UserPrefix>();
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<PostLike> PostLikes => Set<PostLike>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<Follow> Follows => Set<Follow>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrbitDbContext).Assembly);
    }
}
