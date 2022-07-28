using IT_Next.Core.Entities;
using IT_Next.Core.Helpers;
using System.Linq.Expressions;

namespace IT_Next.Core.Repositories;

public interface IRepository<TEntity> where TEntity : BaseEntity
{
    Task AddAsync(TEntity entity);
    Task<TEntity?> GetByIdAsync(int id, params Expression<Func<TEntity, object>>[] includeProperties);
    Task<IEnumerable<TEntity>> GetAsync(Query<TEntity>? query = null);
    Task DeleteAsync(TEntity entity);
    Task<int> GetCountAsync(Expression<Func<TEntity, bool>>? conditions = null);
}