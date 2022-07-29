using IT_Next.Core.Entities;
using IT_Next.Core.Repositories;
using IT_Next.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace IT_Next.UnitTests.Infrastructure.Services;

[TestFixture]
internal class ProductServiceTests
{
    private Mock<IProductRepository> _productRepository;
    private Mock<IPhotoRepository> _photoRepository;
    private Mock<ICategoryRepository> _categoryRepository;
    private ProductService _productService;

    [SetUp]
    public void Setup()
    {
        _productRepository = new Mock<IProductRepository>();
        _photoRepository = new Mock<IPhotoRepository>();
        _categoryRepository = new Mock<ICategoryRepository>();
        _productService = new ProductService(_productRepository.Object,
         _photoRepository.Object, _categoryRepository.Object);
    }

    [Test]
    public async Task IsUnique_ProductWithSameNameAndAnotherIdExisted_ReturnsFalse()
    {
        const string name = "something";
        const int id = 1;
        _productRepository.Setup(scr => scr.GetByNameAsync(name))
            .ReturnsAsync(new Product());

        var result = await _productService.IsUniqueAsync(name, id);

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task IsUnique_ProductWithSameNameSameIdExisted_ReturnsTrue()
    {
        const string name = "something";
        const int id = 1;
        _productRepository.Setup(scr => scr.GetByNameAsync(name))
            .ReturnsAsync(new Product { Id = id });

        var result = await _productService.IsUniqueAsync(name, id);

        Assert.That(result, Is.True);
    }

    [Test]
    public async Task IsUnique_ProductWithSameNameAndSameIdDoesNotExisted_ReturnsTrue()
    {
        const string name = "something";
        const int id = 1;
        _productRepository.Setup(scr => scr.GetByNameAsync(name))
            .ReturnsAsync((Product?)null);

        var result = await _productService.IsUniqueAsync(name, id);

        Assert.That(result, Is.True);
    }

    [Test]
    [TestCase("name", true)]
    [TestCase(null, false)]
    public async Task AddAsync_WhenCalled_ReturnsExpectedResult(string imageName, bool expectedResult)
    {
        var product = new Product();
        IFormFile image = null;
        _photoRepository.Setup(pr => pr.SaveAsync(image!, It.IsAny<string>()))
            .ReturnsAsync(imageName);

        var result = await _productService.AddAsync(product, image!);

        Assert.That(result, Is.EqualTo(expectedResult));
        if (expectedResult)
            Assert.That(product.ImagePath, Is.EqualTo(imageName));
    }

    [Test]
    [TestCase("name", true)]
    [TestCase(null, false)]
    public async Task UpdateImageAsync_WhenCalled_ReturnsExpectedResult(string imageName, bool expectedResult)
    {
        const string oldImageName = "oldName";
        var product = new Product { ImagePath = oldImageName };
        IFormFile image = null;
        _photoRepository.Setup(pr => pr.SaveAsync(image!, It.IsAny<string>()))
            .ReturnsAsync(imageName);

        var result = await _productService.UpdateImageAsync(product, image!);

        Assert.That(result, Is.EqualTo(expectedResult));
        if (expectedResult)
        {
            Assert.That(product.ImagePath, Is.Not.EqualTo(oldImageName));
            Assert.That(product.ImagePath, Is.EqualTo(imageName));
        }
    }

    [Test]
    public void UpdateTime_WhenCalled_ReturnsExpectedResult()
    {
        var product = new Product();

        _productService.UpdateTime(product);

        Assert.That(product.LastUpdate, Is.Not.EqualTo(DateTime.MinValue));
    }

    [Test]
    public void DeleteOldImage_WhenDoesNotOldImageExisted_DoesNotCallPhotoRepository()
    {
        _productService.DeleteOldImage();

        _photoRepository.Verify(ps => ps.Delete(It.IsAny<string>()),
            Times.Never);
    }

    [Test]
    public async Task DeleteOldImage_WhenOldImageExisted_CallPhotoRepositoryToDeleteIt()
    {
        var product = new Product { ImagePath = "" };
        var image = It.IsAny<IFormFile>();
        _photoRepository.Setup(pr => pr.SaveAsync(image, It.IsAny<string>()))
            .ReturnsAsync("anyName");
        await _productService.UpdateImageAsync(product, image);

        _productService.DeleteOldImage();

        _photoRepository.Verify(ps => ps.Delete(It.IsAny<string>()),
            Times.Once);
    }

    [Test]
    public void DeleteImage_WhenCalled_CallPhotoRepositoryToDeleteImage()
    {
        var product = new Product { ImagePath = "any" };

        _productService.DeleteImage(product);

        _photoRepository.Verify(ps => ps.Delete(product.ImagePath),
            Times.Once);
    }

    [Test]
    public async Task IncludeCategoryAsync_WhenCalled_AssignCategoryToProduct()
    {
        var category = new Category { Id = 1, Name = "cat" };
        _categoryRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(category);
        var product = new Product 
        { 
            SubCategory = new SubCategory{ CategoryId = 1, Category = null! }
        };

        await _productService.IncludeCategoryAsync(product);

        Assert.That(product.SubCategory.Category, Is.Not.Null);
        Assert.That(product.SubCategory.Category.Id, Is.EqualTo(category.Id));
        Assert.That(product.SubCategory.Category.Name, Is.EqualTo(category.Name));
    }
}