using IT_Next.Core.Entities;
using IT_Next.Core.Repositories;
using IT_Next.Core.Services;

namespace IT_Next.Infrastructure.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly IPhotoRepository _photoRepository;
    private string? _oldImage;
    private const string ProductImagesFolderName = "products";

    public ProductService(IProductRepository productRepository, IPhotoRepository photoRepository)
    {
        _productRepository = productRepository;
        _photoRepository = photoRepository;
    }

    public async Task<bool> IsUniqueAsync(string name, int id)
    {
        var familiarProduct = await _productRepository.GetByNameAsync(name);

        return familiarProduct == null || familiarProduct.Id == id;
    }

    public async Task<bool> AddAsync(Product product, IFormFile image)
    {
        var photoName = await _photoRepository.SaveAsync(image, ProductImagesFolderName);
        if (photoName == null)
            return false;

        product.ImagePath = photoName;
        UpdateTime(product);

        await _productRepository.AddAsync(product);
        return true;
    }

    public async Task<bool> UpdateImageAsync(Product product, IFormFile image)
    {
        var photoName = await _photoRepository.SaveAsync(image, ProductImagesFolderName);
        if (photoName == null)
            return false;

        _oldImage = product.ImagePath;
        product.ImagePath = photoName;
        UpdateTime(product);

        return true;
    }

    public void DeleteOldImage()
    {
        if (_oldImage != null)
            _photoRepository.Delete(_oldImage);
    }

    public void DeleteImage(Product product)
    {
        _photoRepository.Delete(product.ImagePath);
    }

    public void UpdateTime(Product product)
    {
        product.LastUpdate = DateTime.UtcNow;
    }
}