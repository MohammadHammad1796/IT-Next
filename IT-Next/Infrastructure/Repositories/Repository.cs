using IT_Next.Core.Entities;
using IT_Next.Core.Helpers;
using IT_Next.Core.Repositories;
using IT_Next.Infrastructure.Data;
using IT_Next.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace IT_Next.Infrastructure.Repositories;

public class Repository<TEntity> : IRepository<TEntity> where TEntity : BaseEntity
{
    protected DbSet<TEntity> DbSet;
    protected readonly IQueryCustomization<TEntity> QueryCustomization;
    protected readonly ApplicationDbContext DbContext;

    public Repository(ApplicationDbContext dbContext, IQueryCustomization<TEntity> queryCustomization)
    {
        DbContext = dbContext;
        DbSet = dbContext.Set<TEntity>();
        QueryCustomization = queryCustomization;
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
            queryable = QueryCustomization.ApplyQuery(queryable, query);

        return await queryable.ToListAsync();
    }

    public virtual async Task DeleteAsync(TEntity entity)
    {
        await Task.Run(() =>
        {
            DbSet.Remove(entity);
        });
    }

    public async Task<int> GetCountAsync(Expression<Func<TEntity, bool>>? conditions = null)
    {
        return conditions == null ?
            await DbSet.CountAsync() :
            await DbSet.CountAsync(conditions);
    }
}