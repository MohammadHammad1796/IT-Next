using System.Linq.Expressions;
using IT_Next.Core.Entities;
using IT_Next.Core.Repositories;
using IT_Next.Infrastructure.Data;
using IT_Next.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace IT_Next.Infrastructure.Repositories;

public class UniqueFieldsRepository<TEntity> : Repository<TEntity>, IUniqueFieldsRepository<TEntity>
        where TEntity : EntityWithUniqueName
{
    public UniqueFieldsRepository(ApplicationDbContext dbContext,
        IQueryCustomization<TEntity> queryCustomization)
        : base(dbContext, queryCustomization)
    {
    }

    public virtual async Task<TEntity?> GetByNameAsync(string name, params Expression<Func<TEntity, object>>[] includeProperties)
    {
        //return await DbSet.SingleOrDefaultAsync(c => c.Name == name);
        var queryable = DbSet.Where(c => c.Name == name);
        if (!queryable.Any() || !includeProperties.Any())
            return await queryable.SingleOrDefaultAsync();

        queryable = queryable.Include(includeProperties);

        return await queryable.FirstAsync();
    }
}