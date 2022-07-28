using AutoMapper;
using IT_Next.Controllers.APIs;
using IT_Next.Controllers.APIs.Resources;
using IT_Next.Core.Entities;
using IT_Next.Core.Helpers;
using IT_Next.Core.Repositories;
using IT_Next.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IT_Next.UnitTests.Controllers.APIs;

[TestFixture]
internal class ProductsControllerTests
{
    private ProductsController _productsController;
    private Mock<IMapper> _mapper;
    private Mock<IUnitOfWork> _unitOfWork;
    private Mock<ISubCategoryRepository> _subCategoryRepository;
    private Mock<IBrandRepository> _brandRepository;
    private Mock<IProductService> _productService;
    private Mock<IProductRepository> _productRepository;

    [SetUp]
    public void Setup()
    {
        _subCategoryRepository = new Mock<ISubCategoryRepository>();
        _unitOfWork = new Mock<IUnitOfWork>();
        _mapper = new Mock<IMapper>();
        _brandRepository = new Mock<IBrandRepository>();
        _productService = new Mock<IProductService>();
        _productRepository = new Mock<IProductRepository>();

        _productsController = new ProductsController(_mapper.Object, _unitOfWork.Object,
            _subCategoryRepository.Object, _brandRepository.Object,
            _productService.Object, _productRepository.Object);
    }

    [Test]
    public async Task SaveCheckBehavior_CreateOrUpdateProductWhileSubCategoryDoesNotExisted_ReturnsFalseAndAddModelErrors()
    {
        var saveResource = new SaveProductResource { Name = "something", SubCategoryId = 1 };
        _subCategoryRepository.Setup(cr => cr.GetByIdAsync(saveResource.SubCategoryId))
            .ReturnsAsync((SubCategory?)null);

        var result = await _productsController.SaveCheckBehavior(saveResource);

        Assert.That(result, Is.EqualTo(false));
        Assert.That(_productsController.ModelState.Count, Is.EqualTo(1));
        Assert.That(_productsController.ModelState.ContainsKey(nameof(saveResource.SubCategoryId)), Is.True);
    }

    [Test]
    public async Task SaveCheckBehavior_CreateOrUpdateProductWhileBrandDoesNotExisted_ReturnsFalseAndAddModelErrors()
    {
        var saveResource = new SaveProductResource
        {
            Name = "something",
            SubCategoryId = 1,
            BrandId = 1
        };
        _subCategoryRepository.Setup(cr => cr.GetByIdAsync(saveResource.SubCategoryId))
            .ReturnsAsync(new SubCategory());
        _brandRepository.Setup(cr => cr.GetByIdAsync(saveResource.BrandId))
            .ReturnsAsync((Brand?)null);

        var result = await _productsController.SaveCheckBehavior(saveResource);

        Assert.That(result, Is.EqualTo(false));
        Assert.That(_productsController.ModelState.Count, Is.EqualTo(1));
        Assert.That(_productsController.ModelState.ContainsKey(nameof(saveResource.BrandId)), Is.True);
    }

    [Test]
    public async Task SaveCheckBehavior_CreateOrUpdateProductWhileNameDuplicated_ReturnsFalseAndAddModelErrors()
    {
        var saveResource = new SaveProductResource
        {
            Name = "something",
            SubCategoryId = 1,
            BrandId = 1
        };
        _subCategoryRepository.Setup(cr => cr.GetByIdAsync(saveResource.SubCategoryId))
            .ReturnsAsync(new SubCategory());
        _brandRepository.Setup(br => br.GetByIdAsync(saveResource.BrandId))
            .ReturnsAsync(new Brand());
        _productService.Setup(ps => ps.IsUniqueAsync(saveResource.Name, It.IsAny<int>()))
            .ReturnsAsync(false);

        var result = await _productsController.SaveCheckBehavior(saveResource);

        Assert.That(result, Is.EqualTo(false));
        Assert.That(_productsController.ModelState.Count, Is.EqualTo(1));
        Assert.That(_productsController.ModelState.ContainsKey(nameof(saveResource.Name)), Is.True);
    }

    [Test]
    public async Task SaveCheckBehavior_CreateOrUpdateProductWhileNoProblems_ReturnsTrue()
    {
        var saveResource = new SaveProductResource
        {
            Name = "something",
            SubCategoryId = 1,
            BrandId = 1
        };
        _subCategoryRepository.Setup(cr => cr.GetByIdAsync(saveResource.SubCategoryId))
            .ReturnsAsync(new SubCategory());
        _brandRepository.Setup(br => br.GetByIdAsync(saveResource.BrandId))
            .ReturnsAsync(new Brand());
        _productService.Setup(ps => ps.IsUniqueAsync(saveResource.Name, It.IsAny<int>()))
            .ReturnsAsync(true);

        var result = await _productsController.SaveCheckBehavior(saveResource);

        Assert.That(result, Is.EqualTo(true));
        Assert.That(_productsController.ModelState.Count, Is.EqualTo(0));
    }

    [Test]
    public async Task Add_ServiceAddFail_ReturnsInternalServerError()
    {
        var saveResource = new SaveProductResource();
        _subCategoryRepository.Setup(cr => cr.GetByIdAsync(saveResource.SubCategoryId))
            .ReturnsAsync(new SubCategory());
        _brandRepository.Setup(br => br.GetByIdAsync(saveResource.BrandId))
            .ReturnsAsync(new Brand());
        _productService.Setup(ps => ps.IsUniqueAsync(saveResource.Name, It.IsAny<int>()))
            .ReturnsAsync(true);
        _productService.Setup(ps => ps.AddAsync(It.IsAny<Product>(), saveResource.Image!))
            .ReturnsAsync(false);

        var result = await _productsController.Add(saveResource);

        Assert.That(result, Is.TypeOf<StatusCodeResult>());
        var statusCodeResult = result as StatusCodeResult;
        const int internalServerErrorCode = 500;
        Assert.That(statusCodeResult!.StatusCode, Is.EqualTo(internalServerErrorCode));
    }

    [Test]
    public async Task Add_CheckFail_ReturnsBadRequestWithModelErrors()
    {
        var saveResource = new SaveProductResource();
        _subCategoryRepository.Setup(cr => cr.GetByIdAsync(saveResource.SubCategoryId))
            .ReturnsAsync((SubCategory?)null);

        var result = await _productsController.Add(saveResource);

        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        var badRequestObjectResult = result as BadRequestObjectResult;
        Assert.That(badRequestObjectResult!.Value, Is.TypeOf<SerializableError>());
    }

    [Test]
    public async Task Add_CompleteFail_DeleteSavedImageAndReturnsInternalServerError()
    {
        var saveResource = new SaveProductResource();
        _subCategoryRepository.Setup(cr => cr.GetByIdAsync(saveResource.SubCategoryId))
            .ReturnsAsync(new SubCategory());
        _brandRepository.Setup(br => br.GetByIdAsync(saveResource.BrandId))
            .ReturnsAsync(new Brand());
        _productService.Setup(ps => ps.IsUniqueAsync(saveResource.Name, It.IsAny<int>()))
            .ReturnsAsync(true);
        _productService.Setup(ps => ps.AddAsync(It.IsAny<Product>(), saveResource.Image!))
            .ReturnsAsync(true);
        _unitOfWork.Setup(u => u.CompleteAsync())
            .Throws<Exception>();

        var result = await _productsController.Add(saveResource);

        Assert.That(result, Is.TypeOf<StatusCodeResult>());
        var statusCodeResult = result as StatusCodeResult;
        const int internalServerErrorCode = 500;
        Assert.That(statusCodeResult!.StatusCode, Is.EqualTo(internalServerErrorCode));
        _productService.Verify(ps => ps.DeleteImage(It.IsAny<Product>()), Times.Once);
    }

    [Test]
    public async Task Add_NoProblems_ReturnsOkResultContainsProductResource()
    {
        var saveResource = new SaveProductResource();
        _subCategoryRepository.Setup(cr => cr.GetByIdAsync(saveResource.SubCategoryId))
            .ReturnsAsync(new SubCategory());
        _brandRepository.Setup(br => br.GetByIdAsync(saveResource.BrandId))
            .ReturnsAsync(new Brand());
        _productService.Setup(ps => ps.IsUniqueAsync(saveResource.Name, It.IsAny<int>()))
            .ReturnsAsync(true);
        _productService.Setup(ps => ps.AddAsync(It.IsAny<Product>(), saveResource.Image!))
            .ReturnsAsync(true);
        _mapper.Setup(m => m.Map<ProductResource>(It.IsAny<Product>()))
            .Returns(new ProductResource());

        var result = await _productsController.Add(saveResource);

        Assert.That(result, Is.TypeOf<OkObjectResult>());
        var okObjectResult = result as OkObjectResult;
        Assert.That(okObjectResult!.Value, Is.TypeOf<ProductResource>());
    }

    [Test]
    public async Task Update_ProductDoesNotExisted_ReturnsNotFoundResult()
    {
        _productRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync((Product?)null);

        var result = await _productsController.Update(It.IsAny<SaveProductResource>(), 1);

        Assert.That(result, Is.TypeOf<NotFoundResult>());
    }

    [Test]
    public async Task Update_CheckFail_ReturnsBadRequestWithModelErrors()
    {
        var saveResource = new SaveProductResource { SubCategoryId = 1 };
        _subCategoryRepository.Setup(cr => cr.GetByIdAsync(saveResource.SubCategoryId))
            .ReturnsAsync((SubCategory?)null);
        _productRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new Product());

        var result = await _productsController.Update(saveResource, 1);

        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        var badRequestObjectResult = result as BadRequestObjectResult;
        Assert.That(badRequestObjectResult!.Value, Is.TypeOf<SerializableError>());
    }

    [Test]
    public async Task Update_ResourceHaveImageAndUpdateItFails_ReturnsInternalServerError()
    {
        var image = (new Mock<IFormFile>()).Object;
        var saveResource = new SaveProductResource { Image = image };
        _productRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new Product());
        _subCategoryRepository.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new SubCategory());
        _brandRepository.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new Brand());
        _productService.Setup(s => s.IsUniqueAsync(It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(true);
        _productService.Setup(s => s.UpdateImageAsync(It.IsAny<Product>(), image))
            .ReturnsAsync(false);

        var result = await _productsController.Update(saveResource, 1);

        Assert.That(result, Is.TypeOf<StatusCodeResult>());
        var statusCodeResult = result as StatusCodeResult;
        const int internalServerErrorCode = 500;
        Assert.That(statusCodeResult!.StatusCode, Is.EqualTo(internalServerErrorCode));
    }

    [Test]
    public async Task Update_ResourceDoesNotHaveImageAndPropertiesDoesNotChanged_ReturnsNoContentResult()
    {
        var saveResource = new SaveProductResource { Image = null };
        var productBeforeUpdate = new Product
        {
            Name = "Laptop",
            BrandId = 1,
            SubCategoryId = 1,
            Price = 10,
        };
        var productAfterUpdate = new Product
        {
            Name = productBeforeUpdate.Name,
            BrandId = productBeforeUpdate.BrandId,
            SubCategoryId = productBeforeUpdate.SubCategoryId,
            Price = productBeforeUpdate.Price,
        };
        _productRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(productBeforeUpdate);
        _subCategoryRepository.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new SubCategory());
        _brandRepository.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new Brand());
        _productService.Setup(s => s.IsUniqueAsync(It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(true);
        _mapper.Setup(m => m.Map(saveResource, It.IsAny<Product>()))
            .Returns(productAfterUpdate);

        var result = await _productsController.Update(saveResource, 1);

        Assert.That(result, Is.TypeOf<NoContentResult>());
    }

    [Test]
    public async Task Update_ResourceChangePropertyAtLeastButSaveFailed_TimeUpdatedAndReturnsInternalServerError()
    {
        var saveResource = new SaveProductResource { Image = null };
        var productBeforeUpdate = new Product
        {
            Name = "Laptop",
            BrandId = 1,
            SubCategoryId = 1,
            Price = 10,
        };
        var productAfterUpdate = new Product
        {
            Name = "Laptop",
            BrandId = productBeforeUpdate.BrandId,
            SubCategoryId = productBeforeUpdate.SubCategoryId,
            Price = productBeforeUpdate.Price,
        };
        _productRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(() => productBeforeUpdate);
        _subCategoryRepository.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new SubCategory());
        _brandRepository.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new Brand());
        _productService.Setup(s => s.IsUniqueAsync(It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(true);
        _mapper.Setup(m => m.Map(saveResource, It.IsAny<Product>()))
            .Returns(() =>
            {
                productBeforeUpdate.Name = "Lap";
                return productAfterUpdate;
            });
        _unitOfWork.Setup(u => u.CompleteAsync())
            .Throws<Exception>();

        var result = await _productsController.Update(saveResource, 1);

        _productService.Verify(s => s.UpdateTime(It.IsAny<Product>()),
            Times.Once);
        _productService.Verify(s => s.DeleteImage(It.IsAny<Product>()),
            Times.Once);
        Assert.That(result, Is.TypeOf<StatusCodeResult>());
        var statusCodeResult = result as StatusCodeResult;
        const int internalServerErrorCode = 500;
        Assert.That(statusCodeResult!.StatusCode, Is.EqualTo(internalServerErrorCode));
    }

    [Test]
    public async Task Update_ProductWithImageUpdatedSuccessfully_ReturnsOkObjectResult()
    {
        var image = new Mock<IFormFile>().Object;
        var saveResource = new SaveProductResource { Image = image };
        var productBeforeUpdate = new Product
        {
            Name = "Laptop",
            BrandId = 1,
            SubCategoryId = 1,
            Price = 10,
        };
        var productAfterUpdate = new Product
        {
            Name = "Laptop",
            BrandId = productBeforeUpdate.BrandId,
            SubCategoryId = productBeforeUpdate.SubCategoryId,
            Price = productBeforeUpdate.Price,
        };
        _productRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(() => productBeforeUpdate);
        _subCategoryRepository.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new SubCategory());
        _brandRepository.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new Brand());
        _productService.Setup(s => s.IsUniqueAsync(It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(true);
        _productService.Setup(s => s.UpdateImageAsync(It.IsAny<Product>(), image))
            .ReturnsAsync(true);
        _mapper.Setup(m => m.Map(saveResource, It.IsAny<Product>()))
            .Returns(() =>
            {
                productBeforeUpdate.Name = "Lap";
                return productAfterUpdate;
            });

        var result = await _productsController.Update(saveResource, 1);

        _productService.Verify(s => s.UpdateImageAsync(It.IsAny<Product>(), image),
            Times.Once);
        _productService.Verify(s => s.DeleteOldImage(),
            Times.Once);

        Assert.That(result, Is.TypeOf<OkObjectResult>());
    }

    [Test]
    public async Task Update_ProductWithNoImageUpdatedSuccessfully_ReturnsOkObjectResult()
    {
        var saveResource = new SaveProductResource { Image = null };
        var productBeforeUpdate = new Product
        {
            Name = "Laptop",
            BrandId = 1,
            SubCategoryId = 1,
            Price = 10,
        };
        var productAfterUpdate = new Product
        {
            Name = "Laptop",
            BrandId = productBeforeUpdate.BrandId,
            SubCategoryId = productBeforeUpdate.SubCategoryId,
            Price = productBeforeUpdate.Price,
        };
        _productRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(() => productBeforeUpdate);
        _subCategoryRepository.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new SubCategory());
        _brandRepository.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new Brand());
        _productService.Setup(s => s.IsUniqueAsync(It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(true);
        _mapper.Setup(m => m.Map(saveResource, It.IsAny<Product>()))
            .Returns(() =>
            {
                productBeforeUpdate.Name = "Lap";
                return productAfterUpdate;
            });

        var result = await _productsController.Update(saveResource, 1);

        _productService.Verify(s => s.UpdateTime(It.IsAny<Product>()),
            Times.Once);
        _productService.Verify(s => s.DeleteOldImage(),
            Times.Never);

        Assert.That(result, Is.TypeOf<OkObjectResult>());
    }

    [Test]
    public async Task Get_WhenCalled_ReturnsOkResultContainsProductResourcesWithTotals()
    {
        var products = new List<Product> { new(), new() };
        var productResources = new List<ProductResource> { new(), new() };
        var productQuery = new Query<Product>();
        _productRepository.Setup(cr => cr.GetAsync(productQuery))
            .ReturnsAsync(products);
        _productRepository.Setup(cr => cr.GetCountAsync(productQuery.Conditions))
            .ReturnsAsync(products.Count);
        var queryResource = new ProductQueryResource();
        _mapper.Setup(m => m.Map<Query<Product>>(queryResource))
            .Returns(productQuery);
        _mapper.Setup(m => m.Map<IEnumerable<ProductResource>>(products))
            .Returns(productResources);

        var result = await _productsController.Get(queryResource);

        Assert.That(productQuery.IncludeProperties.Count, Is.EqualTo(2));
        Assert.That(result, Is.TypeOf<OkObjectResult>());
        var objectResult = result as OkObjectResult;
        Assert.That(objectResult, Is.Not.Null);

        Assert.That(objectResult!.Value, Is.TypeOf<ItemsWithCountResource<ProductResource>>());
        var resources = objectResult.Value as ItemsWithCountResource<ProductResource>;
        Assert.That(resources!.Total, Is.EqualTo(2));
        Assert.That(resources.Items.Count, Is.EqualTo(2));
    }

    [Test]
    public async Task GetById_ProductExists_ReturnsOkResultContainsSubCategoryResource()
    {
        var product = new Product { Id = 1, Name = "something" };
        _productRepository.Setup(r => r.GetByIdAsync(1,
                p => p.Brand, p => p.SubCategory.Category))
            .ReturnsAsync(product);
        var productResource = new ProductResource { Id = 1, Name = "something" };
        _mapper.Setup(m => m.Map<ProductResource>(product))
            .Returns(productResource);

        var result = await _productsController.GetById(1);

        Assert.That(result, Is.TypeOf<OkObjectResult>());
        var objectResult = result as OkObjectResult;
        Assert.That(objectResult, Is.Not.Null);

        Assert.That(objectResult!.Value, Is.EqualTo(productResource));
    }

    [Test]
    public async Task GetById_ProductDoesNotExists_ReturnsNotFoundResult()
    {
        _productRepository.Setup(r => r.GetByIdAsync(0,
                p => p.Brand, p => p.SubCategory.Category))
            .ReturnsAsync((Product?)null);

        var result = await _productsController.GetById(0);

        Assert.That(result, Is.TypeOf<NotFoundResult>());
    }

    [Test]
    public async Task Delete_ProductDoesNotExisted_ReturnsNotFound()
    {
        const int id = 1;
        _productRepository.Setup(scr => scr.GetByIdAsync(1))
            .ReturnsAsync((Product?)null);

        var result = await _productsController.Delete(id);

        Assert.That(result, Is.TypeOf<NotFoundResult>());
    }

    [Test]
    public async Task Delete_ExistedProduct_ReturnsNoContent()
    {
        var product = new Product();
        _productRepository.Setup(scr => scr.GetByIdAsync(1))
            .ReturnsAsync(product);

        var result = await _productsController.Delete(1);

        _productRepository.Verify(scr => scr.DeleteAsync(product),
            Times.Once());
        _unitOfWork.Verify(cr => cr.CompleteAsync(),
            Times.Once);
        _productService.Verify(s => s.DeleteImage(product),
            Times.Once);
        Assert.That(result, Is.TypeOf<NoContentResult>());
    }
}