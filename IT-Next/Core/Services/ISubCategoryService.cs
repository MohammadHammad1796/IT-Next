using IT_Next.Core.Entities;

namespace IT_Next.Core.Services;

public interface ISubCategoryService
{
    Task<bool> IsUniqueAsync(string name, int id);
}