using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Orbit.Application.Interfaces;
using Orbit.Domain.Common;
using Orbit.Infrastructure.DbContext;

namespace Orbit.Infrastructure.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
{
    protected readonly OrbitDbContext Context;
    protected readonly DbSet<T> DbSet;

    public GenericRepository(OrbitDbContext context)
    {
        Context = context;
        DbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(Guid id)
    {
        return await DbSet.FindAsync(id);
    }

    public virtual async Task<List<T>> GetAllAsync()
    {
        var query = DbSet.AsQueryable();

        if (typeof(ISoftDeletable).IsAssignableFrom(typeof(T)))
        {
            query = query.Where(e => ((ISoftDeletable)e).IsActive);
        }

        return await query.ToListAsync();
    }

    public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
    {
        var query = DbSet.AsQueryable();

        if (typeof(ISoftDeletable).IsAssignableFrom(typeof(T)))
        {
            query = query.Where(e => ((ISoftDeletable)e).IsActive);
        }

        return await query.FirstOrDefaultAsync(predicate);
    }

    public virtual async Task<List<T>> GetListAsync(Expression<Func<T, bool>> predicate)
    {
        var query = DbSet.AsQueryable();

        if (typeof(ISoftDeletable).IsAssignableFrom(typeof(T)))
        {
            query = query.Where(e => ((ISoftDeletable)e).IsActive);
        }

        return await query.Where(predicate).ToListAsync();
    }

    public virtual async Task CreateAsync(T entity)
    {
        await DbSet.AddAsync(entity);
        await SaveChangesAsync();
    }

    public virtual void Update(T entity)
    {
        DbSet.Update(entity);
    }

    public virtual async Task DeleteAsync(Guid id)
    {
        var entity = await DbSet.FindAsync(id);

        if (entity is null) return;

        if (entity is ISoftDeletable softDeletable)
        {
            softDeletable.IsActive = false;
            DbSet.Update(entity);
        }
        else
        {
            DbSet.Remove(entity);
        }

        await SaveChangesAsync();
    }

    public virtual async Task SaveChangesAsync()
    {
        await Context.SaveChangesAsync();
    }
}
