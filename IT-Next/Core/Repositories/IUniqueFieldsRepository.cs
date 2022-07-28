using IT_Next.Core.Entities;

namespace IT_Next.Core.Repositories;

public interface IUniqueFieldsRepository<TEntity> where TEntity : BaseEntity
{
    Task<TEntity?> GetByNameAsync(string name);
}