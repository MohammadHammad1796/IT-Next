using IT_Next.Core.Entities;
using IT_Next.Core.Repositories;
using IT_Next.Infrastructure.Data;
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

    public virtual async Task<TEntity?> GetByNameAsync(string name)
    {
        return await DbSet.SingleOrDefaultAsync(c => c.Name == name);
    }
}