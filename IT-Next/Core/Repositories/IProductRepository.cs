using IT_Next.Core.Entities;

namespace IT_Next.Core.Repositories;

public interface IProductRepository : IRepository<Product>, IUniqueFieldsRepository<Product>
{
}