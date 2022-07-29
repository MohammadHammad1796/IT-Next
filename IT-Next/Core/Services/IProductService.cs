using IT_Next.Core.Entities;

namespace IT_Next.Core.Services;

public interface IProductService
{
    Task<bool> IsUniqueAsync(string name, int id);

    Task<bool> AddAsync(Product product, IFormFile image);

    Task<bool> UpdateImageAsync(Product product, IFormFile image);

    void DeleteOldImage();

    void DeleteImage(Product product);

    void UpdateTime(Product product);

    Task IncludeCategoryAsync(Product product);
}