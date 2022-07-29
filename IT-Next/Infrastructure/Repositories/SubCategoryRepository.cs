using IT_Next.Core.Entities;
using IT_Next.Core.Repositories;
using IT_Next.Infrastructure.Data;

namespace IT_Next.Infrastructure.Repositories
{
    public class SubCategoryRepository : UniqueFieldsRepository<SubCategory>, ISubCategoryRepository
    {
        public SubCategoryRepository(ApplicationDbContext dbContext,
            IQueryCustomization<SubCategory> queryCustomization)
            : base(dbContext, queryCustomization)
        {
        }
    }
}