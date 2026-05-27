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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrbitDbContext).Assembly);
    }
}
