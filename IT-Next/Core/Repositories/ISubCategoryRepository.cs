using IT_Next.Core.Entities;

namespace IT_Next.Core.Repositories;

public interface ISubCategoryRepository : IRepository<SubCategory>, IUniqueFieldsRepository<SubCategory>
{
}