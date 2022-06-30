using System.Linq.Expressions;

namespace IT_Next.Core.Helpers;

public class Query<TEntity> where TEntity : class
{
    public Expression<Func<TEntity, bool>>? Conditions { get; set; }

    public ICollection<Ordering<TEntity>> Orders { get; }

    public ICollection<Expression<Func<TEntity, object>>> IncludeProperties { get; }

    public Paging? Paging { get; set; }

    public Query()
    {
        IncludeProperties = new List<Expression<Func<TEntity, object>>>();
        Orders = new List<Ordering<TEntity>>();
    }

    public Query(Paging paging)
    {
        Paging = paging;
        IncludeProperties = new List<Expression<Func<TEntity, object>>>();
        Orders = new List<Ordering<TEntity>>();
    }

    public Query(Expression<Func<TEntity, bool>> conditions)
    {
        Conditions = conditions;
        IncludeProperties = new List<Expression<Func<TEntity, object>>>();
        Orders = new List<Ordering<TEntity>>();
    }

    public Query(Expression<Func<TEntity, bool>> conditions, Paging paging)
    {
        Conditions = conditions;
        Paging = paging;
        IncludeProperties = new List<Expression<Func<TEntity, object>>>();
        Orders = new List<Ordering<TEntity>>();
    }

    public void AddIncludeProperty(Expression<Func<TEntity, object>> expression)
    {
        IncludeProperties.Add(expression);
    }

    public void AddOrder(Ordering<TEntity> ordering)
    {
        Orders.Add(ordering);
    }
}