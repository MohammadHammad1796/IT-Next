using IT_Next.Core.Entities;
using IT_Next.Core.Helpers;

namespace IT_Next.Core.Repositories;

public interface IQueryCustomization<TEntity> where TEntity : BaseEntity
{
    IQueryable<TEntity> ApplyQuery(IQueryable<TEntity> queryable, Query<TEntity> query);
}