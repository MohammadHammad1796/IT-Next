using IT_Next.Core.Entities;
using IT_Next.Core.Repositories;
using IT_Next.Infrastructure.Data;

namespace IT_Next.Infrastructure.Repositories;

public class ProductRepository : UniqueFieldsRepository<Product>, IProductRepository
{
    public ProductRepository(ApplicationDbContext dbContext,
        IQueryCustomization<Product> queryCustomization)
        : base(dbContext, queryCustomization)
    {
    }
}