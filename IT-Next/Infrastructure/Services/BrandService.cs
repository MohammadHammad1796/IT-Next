using IT_Next.Core.Repositories;
using IT_Next.Core.Services;

namespace IT_Next.Infrastructure.Services;

public class BrandService : IBrandService
{
    private readonly IBrandRepository _brandRepository;

    public BrandService(IBrandRepository brandRepository)
    {
        _brandRepository = brandRepository;
    }

    public async Task<bool> IsUniqueAsync(string name, int id)
    {
        var familiarBrand = await _brandRepository.GetByNameAsync(name);

        return familiarBrand == null || familiarBrand.Id == id;
    }
}