using IT_Next.Core.Entities;
using IT_Next.Core.Helpers;
using IT_Next.Core.Repositories;
using IT_Next.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IT_Next.Infrastructure.Repositories;

public class CategoryRepository : Repository<Category>, ICategoryRepository
{
    public CategoryRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<Category?> GetByNameAsync(string name)
    {
        return await DbSet.SingleOrDefaultAsync(c => c.Name == name);
    }

    public async Task<int> GetCountAsync(Query<Category>? query = null)
    {
        return query?.Conditions == null ?
            await DbSet.CountAsync() :
            await DbSet.CountAsync(query.Conditions);
    }
}