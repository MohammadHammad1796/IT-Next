using IT_Next.Core.Entities;

namespace IT_Next.Core.Repositories;

public interface IBrandRepository : IRepository<Brand>, IUniqueFieldsRepository<Brand>
{
}