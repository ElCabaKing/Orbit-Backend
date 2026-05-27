using System.Linq.Expressions;
using Orbit.Domain.Common;

namespace Orbit.Application.Interfaces;

public interface IGenericRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id);
    Task<List<T>> GetAllAsync();
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
    Task<List<T>> GetListAsync(Expression<Func<T, bool>> predicate);
    Task CreateAsync(T entity);
    void Update(T entity);
    Task DeleteAsync(Guid id);
    Task SaveChangesAsync();
}
