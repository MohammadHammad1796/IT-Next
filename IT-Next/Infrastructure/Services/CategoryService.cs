using IT_Next.Core.Repositories;
using IT_Next.Core.Services;

namespace IT_Next.Infrastructure.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoryService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<bool> IsUnique(string name, int id)
    {
        var fimiliarCategory = await _categoryRepository.GetByNameAsync(name);

        return fimiliarCategory == null || fimiliarCategory.Id == id;
    }
}