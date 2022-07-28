using IT_Next.Core.Entities;

namespace IT_Next.Core.Repositories;

public interface ICategoryRepository : IRepository<Category>, IUniqueFieldsRepository<Category>
{
}