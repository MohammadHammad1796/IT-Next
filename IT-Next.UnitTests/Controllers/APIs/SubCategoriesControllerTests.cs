using AutoMapper;
using IT_Next.Controllers.APIs;
using IT_Next.Controllers.APIs.Resources;
using IT_Next.Core.Entities;
using IT_Next.Core.Helpers;
using IT_Next.Core.Repositories;
using IT_Next.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace IT_Next.UnitTests.Controllers.APIs;

[TestFixture]
internal class SubCategoriesControllerTests
{
    private SubCategoriesController _subCategoriesController;
    private Mock<IMapper> _mapper;
    private Mock<IUnitOfWork> _unitOfWork;
    private Mock<ISubCategoryRepository> _subCategoryRepository;
    private Mock<ISubCategoryService> _subCategoryService;
    private Mock<ICategoryRepository> _categoryRepository;
    private Mock<IProductRepository> _productRepository;
    private Mock<IBrandRepository> _brandRepository;

    [SetUp]
    public void Setup()
    {
        _subCategoryRepository = new Mock<ISubCategoryRepository>();
        _subCategoryService = new Mock<ISubCategoryService>();
        _unitOfWork = new Mock<IUnitOfWork>();
        _mapper = new Mock<IMapper>();
        _categoryRepository = new Mock<ICategoryRepository>();
        _productRepository = new Mock<IProductRepository>();
        _brandRepository = new Mock<IBrandRepository>();

        _subCategoriesController = new SubCategoriesController(_mapper.Object, _unitOfWork.Object,
            _subCategoryRepository.Object, _subCategoryService.Object, _categoryRepository.Object,
                _productRepository.Object, _brandRepository.Object);
    }

    [Test]
    public async Task Get_WhenTotalOrdered_ReturnsOkResultContainsSubCategoryResourcesWithTotals()
    {
        var subCategories = new List<SubCategory> { new(), new() };
        var subCategoryResources = new List<SubCategoryResource> { new(), new() };
        var subCategoryQuery = new Query<SubCategory>();
        _subCategoryRepository.Setup(cr => cr.GetAsync(subCategoryQuery))
            .ReturnsAsync(subCategories);
        _subCategoryRepository.Setup(cr => cr.GetCountAsync(subCategoryQuery.Conditions))
            .ReturnsAsync(subCategories.Count);
        var queryResource = new SubCategoryQueryResource { IncludeCategory = true };
        _mapper.Setup(m => m.Map<Query<SubCategory>>(queryResource))
            .Returns(subCategoryQuery);
        _mapper.Setup(m => m.Map<IEnumerable<SubCategoryResource>>(subCategories))
            .Returns(subCategoryResources);

        var result = await _subCategoriesController.Get(queryResource);

        Assert.That(subCategoryQuery.IncludeProperties.Count, Is.EqualTo(1));
        Assert.That(result, Is.TypeOf<OkObjectResult>());
        var objectResult = result as OkObjectResult;
        Assert.That(objectResult, Is.Not.Null);

        Assert.That(objectResult!.Value, Is.TypeOf<ItemsWithCountResource<SubCategoryResource>>());
        var resources = objectResult.Value as ItemsWithCountResource<SubCategoryResource>;
        Assert.That(resources!.Total, Is.EqualTo(2));
        Assert.That(resources.Items.Count, Is.EqualTo(2));
    }

    [Test]
    public async Task Get_WhenTotalNotOrdered_ReturnsOkResultContainsSubCategoryResources()
    {
        var subCategories = new List<SubCategory> { new(), new() };
        var subCategoryResources = new List<SubCategoryResource> { new(), new() };
        var subCategoryQuery = new Query<SubCategory>();
        _subCategoryRepository.Setup(cr => cr.GetAsync(subCategoryQuery))
            .ReturnsAsync(subCategories);
        _subCategoryRepository.Setup(cr => cr.GetCountAsync(subCategoryQuery.Conditions))
            .ReturnsAsync(subCategories.Count);
        var queryResource = new SubCategoryQueryResource { IncludeCategory = true };
        _mapper.Setup(m => m.Map<Query<SubCategory>>(queryResource))
            .Returns(subCategoryQuery);
        _mapper.Setup(m => m.Map<IEnumerable<SubCategoryResource>>(subCategories))
            .Returns(subCategoryResources);

        var result = await _subCategoriesController.Get(queryResource, withTotal: false);

        Assert.That(subCategoryQuery.IncludeProperties.Count, Is.EqualTo(1));
        Assert.That(result, Is.TypeOf<OkObjectResult>());
        var objectResult = result as OkObjectResult;
        Assert.That(objectResult, Is.Not.Null);

        Assert.That(objectResult!.Value, Is.TypeOf<List<SubCategoryResource>>());
        var resources = objectResult.Value as IEnumerable<SubCategoryResource>;
        Assert.That(resources!.Count(), Is.EqualTo(2));
    }

    [Test]
    public async Task GetById_SubCategoryExists_ReturnsOkResultContainsSubCategoryResource()
    {
        var subCategory = new SubCategory { Id = 1, Name = "something" };
        _subCategoryRepository.Setup(scr => scr.GetByIdAsync(1, sc => sc.Category))
            .ReturnsAsync(subCategory);
        var subCategoryResource = new SubCategoryResource { Id = 1, Name = "something" };
        _mapper.Setup(m => m.Map<SubCategoryResource>(subCategory))
            .Returns(subCategoryResource);

        var result = await _subCategoriesController.GetById(1);

        Assert.That(result, Is.TypeOf<OkObjectResult>());
        var objectResult = result as OkObjectResult;
        Assert.That(objectResult, Is.Not.Null);

        Assert.That(objectResult?.Value, Is.EqualTo(subCategoryResource));
    }

    [Test]
    public async Task GetById_SubCategoryDoesNotExists_ReturnsNotFoundResult()
    {
        _subCategoryRepository.Setup(scr => scr.GetByIdAsync(0, sc => sc.Category))
            .ReturnsAsync((SubCategory?)null);

        var result = await _subCategoriesController.GetById(1);

        Assert.That(result, Is.TypeOf<NotFoundResult>());
    }

    [Test]
    public async Task Save_UpdateNotExistedSubCategory_ReturnsNotFoundResult()
    {
        var saveSubCategoryResource = new SaveSubCategoryResource { Id = 1, Name = "something", CategoryId = 1 };
        _subCategoryRepository.Setup(scr => scr.GetByIdAsync(saveSubCategoryResource.Id))
            .ReturnsAsync((SubCategory?)null);

        var result = await _subCategoriesController.Save(saveSubCategoryResource);

        Assert.That(result, Is.TypeOf<NotFoundResult>());
    }

    [Test]
    [TestCase(0)]
    [TestCase(1)]
    public async Task Save_CreateOrUpdateSubCategoryWhileCategoryDoesNotExisted_ReturnsBadRequestResultWithModelErrors(int id)
    {
        var saveSubCategoryResource = new SaveSubCategoryResource { Id = id, Name = "something", CategoryId = 1 };
        var subCategory = new SubCategory { Id = saveSubCategoryResource.Id, Name = saveSubCategoryResource.Name };
        _subCategoryRepository.Setup(cr => cr.GetByIdAsync(saveSubCategoryResource.Id))
            .ReturnsAsync(subCategory);
        _categoryRepository.Setup(cr => cr.GetByIdAsync(saveSubCategoryResource.CategoryId))
            .ReturnsAsync((Category?)null);

        var result = await _subCategoriesController.Save(saveSubCategoryResource);

        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        var objectResult = result as BadRequestObjectResult;
        Assert.That(objectResult, Is.Not.Null);
        var modelState = objectResult?.Value as Dictionary<string, object>;
        Assert.That(modelState?.Count, Is.EqualTo(1));
        Assert.That(modelState?.ContainsKey(nameof(saveSubCategoryResource.CategoryId)), Is.True);
    }

    [Test]
    [TestCase(0)]
    [TestCase(1)]
    public async Task Save_CreateOrUpdateSubCategoryWithDuplicatedName_ReturnsBadRequestResultWithModelErrors(int id)
    {
        var saveSubCategoryResource = new SaveSubCategoryResource { Id = id, Name = "something", CategoryId = 1 };
        var subCategory = new SubCategory { Id = id, Name = saveSubCategoryResource.Name };
        _subCategoryRepository.Setup(scr => scr.GetByIdAsync(1))
            .ReturnsAsync(subCategory);
        _categoryRepository.Setup(cr => cr.GetByIdAsync(1))
            .ReturnsAsync(new Category());
        _subCategoryService.Setup(scs => scs
                .IsUniqueAsync(saveSubCategoryResource.Name, saveSubCategoryResource.Id))
            .ReturnsAsync(false);

        var result = await _subCategoriesController.Save(saveSubCategoryResource);

        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        var objectResult = result as BadRequestObjectResult;
        Assert.That(objectResult, Is.Not.Null);
        var modelState = objectResult?.Value as Dictionary<string, object>;
        Assert.That(modelState?.ContainsKey(nameof(saveSubCategoryResource.Name)), Is.True);
    }

    [Test]
    public async Task Save_CreateSubCategory_ReturnsOkResultContainsSubCategoryResource()
    {
        var saveSubCategoryResource = new SaveSubCategoryResource { Id = 0, Name = "something", CategoryId = 1 };
        var subCategory = new SubCategory { Id = saveSubCategoryResource.Id, Name = saveSubCategoryResource.Name };
        var subCategoryResource = new SubCategoryResource { Id = saveSubCategoryResource.Id, Name = saveSubCategoryResource.Name, CategoryName = "name" };
        _subCategoryService.Setup(scs => scs
                .IsUniqueAsync(saveSubCategoryResource.Name, saveSubCategoryResource.Id))
            .ReturnsAsync(true);
        _categoryRepository.Setup(cr => cr.GetByIdAsync(saveSubCategoryResource.CategoryId))
            .ReturnsAsync(new Category());
        _mapper.Setup(m => m.Map(saveSubCategoryResource, It.IsAny<SubCategory>()))
            .Returns(subCategory);
        _subCategoryRepository.Setup(scr => scr.AddAsync(subCategory))
            .Callback(() => { subCategory.Id = 1; });
        _mapper.Setup(m => m.Map<SubCategoryResource>(It.IsAny<SubCategory>()))
            .Returns(() =>
            {
                subCategoryResource.Id = subCategory.Id;
                return subCategoryResource;
            });

        var result = await _subCategoriesController.Save(saveSubCategoryResource);

        _subCategoryRepository.Verify(scr => scr.AddAsync(It.IsAny<SubCategory>()),
            Times.Once);
        _unitOfWork.Verify(cr => cr.CompleteAsync(),
            Times.Once);

        Assert.That(result, Is.TypeOf<OkObjectResult>());
        var objectResult = result as OkObjectResult;
        Assert.That(objectResult, Is.Not.Null);
        Assert.That(objectResult?.Value, Is.EqualTo(subCategoryResource));
    }

    [Test]
    public async Task Save_UpdateSubCategory_ReturnsOkResultContainsSubCategoryResource()
    {
        var saveSubCategoryResource = new SaveSubCategoryResource { Id = 1, Name = "something", CategoryId = 1 };
        var subCategory = new SubCategory { Id = saveSubCategoryResource.Id, Name = saveSubCategoryResource.Name };
        var subCategoryResource = new SubCategoryResource { Id = saveSubCategoryResource.Id, Name = saveSubCategoryResource.Name, CategoryName = "name" };
        _subCategoryService.Setup(scs => scs
                .IsUniqueAsync(saveSubCategoryResource.Name, saveSubCategoryResource.Id))
            .ReturnsAsync(true);
        _subCategoryRepository.Setup(scr => scr.GetByIdAsync(saveSubCategoryResource.Id))
            .ReturnsAsync(subCategory);
        _categoryRepository.Setup(cr => cr.GetByIdAsync(saveSubCategoryResource.CategoryId))
            .ReturnsAsync(new Category());
        _mapper.Setup(m => m.Map(saveSubCategoryResource, It.IsAny<SubCategory>()))
            .Returns(subCategory);
        _subCategoryRepository.Setup(scr => scr.AddAsync(subCategory))
            .Callback(() => { subCategory.Id = 1; });
        _mapper.Setup(m => m.Map<SubCategoryResource>(It.IsAny<SubCategory>()))
            .Returns(() => subCategoryResource);

        var result = await _subCategoriesController.Save(saveSubCategoryResource);

        _subCategoryRepository.Verify(scr => scr.AddAsync(It.IsAny<SubCategory>()),
            Times.Never);
        _unitOfWork.Verify(cr => cr.CompleteAsync(),
            Times.Once);

        Assert.That(result, Is.TypeOf<OkObjectResult>());
        var objectResult = result as OkObjectResult;
        Assert.That(objectResult, Is.Not.Null);
        Assert.That(objectResult?.Value, Is.EqualTo(subCategoryResource));
    }

    [Test]
    public async Task Delete_CategoryDoesNotExisted_ReturnsNotFound()
    {
        const int id = 1;
        _subCategoryRepository.Setup(scr => scr.GetByIdAsync(1))
            .ReturnsAsync((SubCategory?)null);

        var result = await _subCategoriesController.Delete(id);

        Assert.That(result, Is.TypeOf<NotFoundResult>());
    }

    [Test]
    public async Task Delete_ExistedCategory_ReturnsNoContent()
    {
        var subCategory = new SubCategory { Id = 1, Name = "something" };
        _subCategoryRepository.Setup(scr => scr.GetByIdAsync(subCategory.Id))
            .ReturnsAsync(subCategory);

        var result = await _subCategoriesController.Delete(subCategory.Id);

        _subCategoryRepository.Verify(scr => scr.DeleteAsync(subCategory),
            Times.Once());
        _unitOfWork.Verify(cr => cr.CompleteAsync(),
            Times.Once);
        Assert.That(result, Is.TypeOf<NoContentResult>());
    }

    [Test]
    public async Task GetBrands_SubCategoryDoesNotExists_ReturnsNotFoundResult()
    {
        const string subCategoryName = "test";
        _subCategoryRepository.Setup(scr => scr.GetByNameAsync(subCategoryName))
            .ReturnsAsync((SubCategory?)null);

        var result = await _subCategoriesController.GetBrands(subCategoryName);

        Assert.That(result, Is.TypeOf<NotFoundResult>());
    }

    [Test]
    public async Task GetBrands_SubCategoryExists_ReturnsOkResultContainsBrandsResources()
    {
        var subCategory = new SubCategory { Id = 1, Name = "something" };
        _subCategoryRepository.Setup(scr => scr.GetByNameAsync(subCategory.Name))
            .ReturnsAsync(subCategory);
        var brands = new List<Brand>();
        var brandsResources = new List<BrandResource>();
        _brandRepository.Setup(br => br.GetAsync(new Query<Brand>()
        {
            Conditions = b => b.Products.Any(p => p.SubCategoryId == 1 && p.Quantity > 0)
        }))
            .ReturnsAsync(brands);
        _mapper.Setup(m => m.Map<IEnumerable<BrandResource>>(brands))
            .Returns(brandsResources);

        var result = await _subCategoriesController.GetBrands(subCategory.Name);

        Assert.That(result, Is.TypeOf<OkObjectResult>());
        var objectResult = result as OkObjectResult;
        Assert.That(objectResult, Is.Not.Null);

        Assert.That(objectResult?.Value, Is.EqualTo(brandsResources));
    }

    [Test]
    public async Task GetMinimumAndMaximumPrice_SubCategoryDoesNotExists_ReturnsNotFoundResult()
    {
        const string subCategoryName = "test";
        _subCategoryRepository.Setup(scr => scr.GetByNameAsync(subCategoryName))
            .ReturnsAsync((SubCategory?)null);

        var result = await _subCategoriesController.GetMinimumAndMaximumPrice(subCategoryName);

        Assert.That(result, Is.TypeOf<NotFoundResult>());
    }

    [Test]
    public async Task GetMinimumAndMaximumPrice_SubCategoryDoesNotHaveAvailableProducts_ReturnsOkResultContainsDefaultMinimumMaximumPriceResource()
    {
        var subCategory = new SubCategory { Id = 1, Name = "something" };
        _subCategoryRepository.Setup(scr => scr.GetByNameAsync(subCategory.Name))
            .ReturnsAsync(subCategory);

        var resource = new MinimumMaximumPriceResource();
        _productRepository.Setup(pr => pr.GetAsync(It.IsAny<Query<Product>>()))
            .ReturnsAsync(new List<Product>());

        var result = await _subCategoriesController.GetMinimumAndMaximumPrice(subCategory.Name);

        Assert.That(result, Is.TypeOf<OkObjectResult>());
        var objectResult = result as OkObjectResult;
        Assert.That(objectResult, Is.Not.Null);

        var obj = objectResult!.Value as MinimumMaximumPriceResource;
        Assert.That(obj!.Minimum, Is.EqualTo(resource.Minimum));
        Assert.That(obj.Maximum, Is.EqualTo(resource.Maximum));
    }

    [Test]
    public async Task GetMinimumAndMaximumPrice_SubCategoryHaveAvailableProducts_ReturnsOkResultContainsMinimumMaximumPriceResource()
    {
        var subCategory = new SubCategory { Id = 1, Name = "something" };
        _subCategoryRepository.Setup(scr => scr.GetByNameAsync(subCategory.Name))
            .ReturnsAsync(subCategory);
        var minimumProductPrice = new Product()
        {
            Price = 50,
            Discount = 0.1f
        };
        _productRepository.Setup(pr => pr.GetAsync(It.IsAny<Query<Product>>()))
            .ReturnsAsync(new List<Product>()
            {
                minimumProductPrice
            });

        var result = await _subCategoriesController.GetMinimumAndMaximumPrice(subCategory.Name);

        Assert.That(result, Is.TypeOf<OkObjectResult>());
        var objectResult = result as OkObjectResult;
        Assert.That(objectResult, Is.Not.Null);

        var resourceResult = objectResult!.Value as MinimumMaximumPriceResource;
        Assert.That(resourceResult!.Minimum, Is.EqualTo(minimumProductPrice.Price * (1 - minimumProductPrice.Discount)));
        Assert.That(resourceResult.Maximum, Is.EqualTo(minimumProductPrice.Price * (1 - minimumProductPrice.Discount)));
    }

    [Test]
    public async Task GetRelatedSubCategories_SubCategoryDoesNotExists_ReturnsNotFoundResult()
    {
        const string subCategoryName = "test";
        _subCategoryRepository.Setup(scr => scr.GetByNameAsync(subCategoryName))
            .ReturnsAsync((SubCategory?)null);

        var result = await _subCategoriesController.GetRelatedSubCategories(subCategoryName);

        Assert.That(result, Is.TypeOf<NotFoundResult>());
    }

    [Test]
    public async Task GetRelatedSubCategories_SubCategoryHaveRelatedThatHaveAvailableProducts_ReturnsOkResultContainsListOfSUbCategoriesNames()
    {
        const string subCategoryName = "test";
        _subCategoryRepository.Setup(scr => scr.GetByNameAsync(subCategoryName))
            .ReturnsAsync(new SubCategory());
        var subCategories = new List<SubCategory>()
        {
            new SubCategory()
            {
                Name = "test1"
            },
            new SubCategory()
            {
                Name = "test2"
            }
        };
        _subCategoryRepository.Setup(scr => scr.GetAsync(It.IsAny<Query<SubCategory>>()))
            .ReturnsAsync(subCategories);

        var result = await _subCategoriesController.GetRelatedSubCategories(subCategoryName);

        Assert.That(result, Is.TypeOf<OkObjectResult>());
        var objectResult = result as OkObjectResult;
        Assert.That(objectResult, Is.Not.Null);
        Assert.That(objectResult!.Value, Is.EqualTo(subCategories.Select(sc => sc.Name).ToList()));
    }

    [Test]
    public async Task GetProducts_SubCategoryDoesNotExists_ReturnsNotFoundResult()
    {
        const string subCategoryName = "test";
        _subCategoryRepository.Setup(scr => scr.GetByNameAsync(subCategoryName))
            .ReturnsAsync((SubCategory?)null);

        var result = await _subCategoriesController.GetProducts(subCategoryName, It.IsAny<SubCategoryProductsQueryResource>());

        Assert.That(result, Is.TypeOf<NotFoundResult>());
    }

    [Test]
    public async Task GetProducts_SubCategoryExisted_ReturnsOkResultContainsProductsResourcesWithTotal()
    {
        var subCategory = new SubCategory { Id = 1, Name = "test" };
        _subCategoryRepository.Setup(cr => cr.GetByNameAsync(subCategory.Name))
            .ReturnsAsync(subCategory);
        _productRepository.Setup(pr => pr.GetAsync(It.IsAny<Query<Product>>()))
            .ReturnsAsync(new List<Product>());
        _productRepository.Setup(pr => pr.GetCountAsync(It.IsAny<Expression<Func<Product, bool>>>()))
            .ReturnsAsync(1);

        var result = await _subCategoriesController.GetProducts(subCategory.Name, It.IsAny<SubCategoryProductsQueryResource>());

        Assert.That(result, Is.TypeOf<OkObjectResult>());
        var objectResult = result as OkObjectResult;
        Assert.That(objectResult, Is.Not.Null);

        Assert.That(objectResult!.Value, Is.TypeOf<ItemsWithCountResource<ProductInSubCategoryProductsListResource>>());
        var resources = objectResult.Value as ItemsWithCountResource<ProductInSubCategoryProductsListResource>;
        Assert.That(resources!.Total, Is.EqualTo(1));
        Assert.That(resources.Items.Count, Is.EqualTo(0));
    }
}