using IT_Next.Core.Entities;
using IT_Next.Core.Repositories;
using IT_Next.Infrastructure.Data;

namespace IT_Next.Infrastructure.Repositories;

public class BrandRepository : UniqueFieldsRepository<Brand>, IBrandRepository
{
    public BrandRepository(ApplicationDbContext dbContext,
        IQueryCustomization<Brand> queryCustomization)
        : base(dbContext, queryCustomization)
    {
    }
}