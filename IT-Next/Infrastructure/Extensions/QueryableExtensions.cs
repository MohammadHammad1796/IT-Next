using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace IT_Next.Infrastructure.Extensions;

public static class QueryableExtensions
{
    public static IQueryable<TEntity> Include<TEntity>(this IQueryable<TEntity> queryable,
        ICollection<Expression<Func<TEntity, object>>> properties) where TEntity : class
    {
        return properties
            .Aggregate(queryable, (entityQueryable, property)
                => entityQueryable.Include(property));
    }
}