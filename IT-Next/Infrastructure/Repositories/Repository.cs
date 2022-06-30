using IT_Next.Core.Entities;
using IT_Next.Core.Helpers;
using IT_Next.Core.Repositories;
using IT_Next.Extensions;
using IT_Next.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace IT_Next.Infrastructure.Repositories;

public class Repository<TEntity> : IRepository<TEntity> where TEntity : BaseEntity
{
    protected DbSet<TEntity> DbSet;
    private readonly QueryCustomization<TEntity> _queryCustomization;

    public Repository(ApplicationDbContext dbContext)
    {
        DbSet = dbContext.Set<TEntity>();
        _queryCustomization = new QueryCustomization<TEntity>();
    }

    public virtual async Task AddAsync(TEntity entity)
    {
        await DbSet.AddAsync(entity);
    }

    public virtual async Task<TEntity?> GetByIdAsync(int id,
        params Expression<Func<TEntity, object>>[] includeProperties)
    {
        var queryable = DbSet.Where(e => e.Id == id);
        if (!queryable.Any() || !includeProperties.Any())
            return await queryable.SingleOrDefaultAsync();

        queryable = queryable.Include(includeProperties);

        return await queryable.FirstAsync();
    }

    public virtual async Task<IEnumerable<TEntity>> GetAsync(Query<TEntity>? query = null)
    {
        IQueryable<TEntity> queryable = DbSet;

        if (query != null)
            queryable = _queryCustomization.ApplyQuery(queryable, query);

        return await queryable.ToListAsync();
    }

    public virtual async Task DeleteAsync(TEntity entity)
    {
        await Task.Run(() =>
        {
            DbSet.Remove(entity);
        });
    }
}