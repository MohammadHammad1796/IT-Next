using IT_Next.Core.Helpers;

namespace IT_Next.Core.Extensions;

public static class QueryableExtensions
{
    public static IQueryable<TEntity> ApplyPaging<TEntity>(this IQueryable<TEntity> queryable, Paging paging)
        where TEntity : class
    {
        var pageIndex = paging.Number - 1;
        var count = pageIndex * paging.Size;
        return queryable.Skip(count).Take(paging.Size);
    }

    public static IQueryable<TEntity> ApplyOrders<TEntity>(this IQueryable<TEntity> queryable,
        ICollection<Ordering<TEntity>> orderings) where TEntity : class
    {
        if (!orderings.Any())
            return queryable;

        var firstOrdering = orderings.First();
        var result = firstOrdering.IsAscending ?
            queryable.OrderBy(firstOrdering.OrderBy) :
            queryable.OrderByDescending(firstOrdering.OrderBy);

        for (var i = 1; i < orderings.Count; i++)
        {
            var ordering = orderings.ElementAt(i);
            result = ordering.IsAscending ?
                result.ThenBy(ordering.OrderBy) :
                result.ThenByDescending(ordering.OrderBy);
        }

        return result;
    }
}