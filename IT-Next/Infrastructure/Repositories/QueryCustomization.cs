using IT_Next.Core.Entities;
using IT_Next.Core.Helpers;
using IT_Next.Extensions;
using IT_Next.Infrastructure.Extensions;

namespace IT_Next.Infrastructure.Repositories;

public class QueryCustomization<TEntity> where TEntity : BaseEntity
{
    public virtual IQueryable<TEntity> ApplyQuery(IQueryable<TEntity> queryable, Query<TEntity> query)
    {
        if (query.Conditions != null)
            queryable = queryable.Where(query.Conditions);

        queryable = queryable.ApplyOrders(query.Orders);

        queryable = queryable.Include(query.IncludeProperties);

        return query.Paging == null ? queryable : queryable.ApplyPaging(query.Paging);
    }
}