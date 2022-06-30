using System.Linq.Expressions;

namespace IT_Next.Core.Helpers;

public class Ordering<TEntity> where TEntity : class
{
    public Expression<Func<TEntity, object>> OrderBy { get; set; }

    public bool IsAscending { get; set; }

    public Ordering(Expression<Func<TEntity, object>> orderBy, bool isAscending)
    {
        OrderBy = orderBy;
        IsAscending = isAscending;
    }
}