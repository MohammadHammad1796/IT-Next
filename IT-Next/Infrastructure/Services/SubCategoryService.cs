using IT_Next.Core.Repositories;
using IT_Next.Core.Services;

namespace IT_Next.Infrastructure.Services;

public class SubCategoryService : ISubCategoryService
{
    private readonly ISubCategoryRepository _subCategoryRepository;

    public SubCategoryService(ISubCategoryRepository subCategoryRepository)
    {
        _subCategoryRepository = subCategoryRepository;
    }

    public async Task<bool> IsUniqueAsync(string name, int id)
    {
        var familiarSubCategory = await _subCategoryRepository.GetByNameAsync(name);

        return familiarSubCategory == null || familiarSubCategory.Id == id;
    }
}