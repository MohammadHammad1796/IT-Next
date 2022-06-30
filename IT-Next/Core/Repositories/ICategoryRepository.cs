using IT_Next.Core.Entities;
using IT_Next.Core.Helpers;

namespace IT_Next.Core.Repositories;

public interface ICategoryRepository : IRepository<Category>
{
    Task<Category?> GetByNameAsync(string name);

    Task<int> GetCountAsync(Query<Category>? query = null);
}