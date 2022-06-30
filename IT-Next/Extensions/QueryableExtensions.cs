using IT_Next.Core.Helpers;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace IT_Next.Extensions;

public static class QueryableExtensions
{
    public static IQueryable<TEntity> Include<TEntity>(this IQueryable<TEntity> queryable,
        ICollection<Expression<Func<TEntity, object>>> properties) where TEntity : class
    {
        return properties
            .Aggregate(queryable, (entityQueryable, property)
                => entityQueryable.Include(property));
    }

    public static IQueryable<TEntity> ApplyPaging<TEntity>(this IQueryable<TEntity> queryable, Paging paging)
        where TEntity : class
    {
        return queryable
            .Skip((paging.Number - 1) * paging.Size)
            .Take(paging.Size);
    }

    public static IQueryable<TEntity> ApplyOrders<TEntity>(this IQueryable<TEntity> queryable,
        ICollection<Ordering<TEntity>> orderings) where TEntity : class
    {
        if (!orderings.Any())
            return queryable;

        var result = orderings.First().IsAscending ?
            queryable.OrderBy(orderings.First().OrderBy) :
            queryable.OrderByDescending(orderings.First().OrderBy);

        for (var i = 1; i < orderings.Count; i++)
            result = orderings.ElementAt(i).IsAscending ?
                result.ThenBy(orderings.ElementAt(i).OrderBy) :
                result.ThenByDescending(orderings.ElementAt(i).OrderBy);

        return result;
    }
}