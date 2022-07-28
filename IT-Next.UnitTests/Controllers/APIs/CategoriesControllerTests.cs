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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IT_Next.UnitTests.Controllers.APIs;

[TestFixture]
internal class CategoriesControllerTests
{
    private CategoriesController _categoriesController;
    private Mock<IMapper> _mapper;
    private Mock<IUnitOfWork> _unitOfWork;
    private Mock<ICategoryRepository> _categoryRepository;
    private Mock<ICategoryService> _categoryService;

    [SetUp]
    public void Setup()
    {
        _categoryRepository = new Mock<ICategoryRepository>();
        _categoryService = new Mock<ICategoryService>();
        _unitOfWork = new Mock<IUnitOfWork>();
        _mapper = new Mock<IMapper>();

        _categoriesController = new CategoriesController(_mapper.Object, _unitOfWork.Object,
            _categoryRepository.Object, _categoryService.Object);
    }

    [Test]
    public async Task Get_WhenTotalOrdered_ReturnsOkResultContainsCategoryResourcesWithTotals()
    {
        var categories = new List<Category> { new(), new() };
        var categoriesResources = new List<CategoryResource> { new(), new() };
        var categoryQuery = new Query<Category>();
        _categoryRepository.Setup(cr => cr.GetAsync(categoryQuery))
            .ReturnsAsync(categories);
        _categoryRepository.Setup(cr => cr.GetCountAsync(categoryQuery.Conditions))
            .ReturnsAsync(categories.Count);
        var queryResource = new CategoryQueryResource();
        _mapper.Setup(m => m.Map<Query<Category>>(queryResource))
            .Returns(categoryQuery);
        _mapper.Setup(m => m.Map<IEnumerable<CategoryResource>>(categories))
            .Returns(categoriesResources);

        var result = await _categoriesController.Get(queryResource);

        Assert.That(result, Is.TypeOf<OkObjectResult>());
        var objectResult = result as OkObjectResult;
        Assert.That(objectResult, Is.Not.Null);

        Assert.That(objectResult!.Value, Is.TypeOf<ItemsWithCountResource<CategoryResource>>());
        var resources = objectResult.Value as ItemsWithCountResource<CategoryResource>;
        Assert.That(resources!.Total, Is.EqualTo(2));
        Assert.That(resources.Items.Count, Is.EqualTo(2));
    }

    [Test]
    public async Task Get_WhenTotalNotOrdered_ReturnsOkResultContainsCategoryResources()
    {
        var categories = new List<Category> { new(), new() };
        var categoriesResources = new List<CategoryResource> { new(), new() };
        var categoryQuery = new Query<Category>();
        _categoryRepository.Setup(cr => cr.GetAsync(categoryQuery))
            .ReturnsAsync(categories);
        _categoryRepository.Setup(cr => cr.GetCountAsync(categoryQuery.Conditions))
            .ReturnsAsync(categories.Count);
        var queryResource = new CategoryQueryResource();
        _mapper.Setup(m => m.Map<Query<Category>>(queryResource))
            .Returns(categoryQuery);
        _mapper.Setup(m => m.Map<IEnumerable<CategoryResource>>(categories))
            .Returns(categoriesResources);

        var result = await _categoriesController.Get(queryResource, withTotal: false);

        Assert.That(result, Is.TypeOf<OkObjectResult>());
        var objectResult = result as OkObjectResult;
        Assert.That(objectResult, Is.Not.Null);

        Assert.That(objectResult!.Value, Is.TypeOf<List<CategoryResource>>());
        var resources = objectResult.Value as IEnumerable<CategoryResource>;
        Assert.That(resources!.Count(), Is.EqualTo(2));
    }

    [Test]
    public async Task GetById_CategoryExists_ReturnsOkResultContainsCategoryResource()
    {
        var category = new Category { Id = 1, Name = "something" };
        _categoryRepository.Setup(cr => cr.GetByIdAsync(1))
            .ReturnsAsync(category);
        var categoryResource = new CategoryResource { Id = 1, Name = "something" };
        _mapper.Setup(m => m.Map<CategoryResource>(category))
            .Returns(categoryResource);

        var result = await _categoriesController.GetById(1);

        Assert.That(result, Is.TypeOf<OkObjectResult>());
        var objectResult = result as OkObjectResult;
        Assert.That(objectResult, Is.Not.Null);

        Assert.That(objectResult?.Value, Is.EqualTo(categoryResource));
    }

    [Test]
    public async Task GetById_CategoryDoesNotExists_ReturnsNotFoundResult()
    {
        _categoryRepository.Setup(cr => cr.GetByIdAsync(0))
            .ReturnsAsync((Category?)null);

        var result = await _categoriesController.GetById(1);

        Assert.That(result, Is.TypeOf<NotFoundResult>());
    }

    [Test]
    public async Task Save_UpdateNotExistedCategory_ReturnsNotFoundResult()
    {
        var categoryResource = new CategoryResource { Id = 1, Name = "something" };
        _categoryRepository.Setup(cr => cr.GetByIdAsync(categoryResource.Id))
            .ReturnsAsync((Category?)null);

        var result = await _categoriesController.Save(categoryResource);

        Assert.That(result, Is.TypeOf<NotFoundResult>());
    }

    [Test]
    public async Task Save_CreateCategory_ReturnsOkResultContainsCategoryResource()
    {
        var categoryResource = new CategoryResource { Id = 0, Name = "something" };
        var category = new Category { Id = categoryResource.Id, Name = categoryResource.Name };
        _categoryService.Setup(cs => cs
                .IsUnique(categoryResource.Name, categoryResource.Id))
            .ReturnsAsync(true);
        _mapper.Setup(m => m.Map(categoryResource, It.IsAny<Category>()))
            .Returns(category);
        _categoryRepository.Setup(cr => cr.AddAsync(category))
            .Callback(() => { category.Id = 1; });
        _mapper.Setup(m => m.Map(It.IsAny<Category>(), It.IsAny<CategoryResource>()))
            .Returns(() =>
            {
                categoryResource.Id = category.Id;
                return categoryResource;
            });

        var result = await _categoriesController.Save(categoryResource);

        _categoryRepository.Verify(cr => cr.AddAsync(It.IsAny<Category>()),
            Times.Once);
        _unitOfWork.Verify(cr => cr.CompleteAsync(),
            Times.Once);

        Assert.That(result, Is.TypeOf<OkObjectResult>());
        var objectResult = result as OkObjectResult;
        Assert.That(objectResult, Is.Not.Null);
        Assert.That(objectResult?.Value, Is.EqualTo(categoryResource));
    }

    [Test]
    [TestCase(0)]
    [TestCase(1)]
    public async Task Save_CreateOrUpdateCategoryWithDuplicatedName_ReturnsBadRequestResultWithModelErrors(int id)
    {
        var categoryResource = new CategoryResource { Id = id, Name = "something" };
        var category = new Category { Id = id, Name = categoryResource.Name };
        _categoryRepository.Setup(cr => cr.GetByIdAsync(1))
            .ReturnsAsync(category);
        _categoryService.Setup(cs => cs
                .IsUnique(categoryResource.Name, categoryResource.Id))
            .ReturnsAsync(false);

        var result = await _categoriesController.Save(categoryResource);

        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        var objectResult = result as BadRequestObjectResult;
        Assert.That(objectResult, Is.Not.Null);
        var modelState = objectResult?.Value as Dictionary<string, object>;
        Assert.That(modelState?.ContainsKey(nameof(categoryResource.Name)), Is.True);
    }

    [Test]
    public async Task Save_UpdateCategory_ReturnsOkResultContainsCategoryResource()
    {
        var categoryResource = new CategoryResource { Id = 1, Name = "something" };
        var category = new Category { Id = categoryResource.Id, Name = categoryResource.Name };
        _categoryRepository.Setup(cr => cr.GetByIdAsync(categoryResource.Id))
            .ReturnsAsync(category);
        _categoryService.Setup(cs => cs
                .IsUnique(categoryResource.Name, categoryResource.Id))
            .ReturnsAsync(true);
        _mapper.Setup(m => m.Map(categoryResource, It.IsAny<Category>()))
            .Returns(category);
        _mapper.Setup(m => m.Map(It.IsAny<Category>(), It.IsAny<CategoryResource>()))
            .Returns(() =>
            {
                categoryResource.Id = category.Id;
                return categoryResource;
            });

        var result = await _categoriesController.Save(categoryResource);

        _categoryRepository.Verify(cr => cr.AddAsync(It.IsAny<Category>()),
            Times.Never);

        Assert.That(result, Is.TypeOf<OkObjectResult>());
        var objectResult = result as OkObjectResult;
        Assert.That(objectResult, Is.Not.Null);
        Assert.That(objectResult?.Value, Is.EqualTo(categoryResource));
    }

    [Test]
    public async Task Delete_CategoryDoesNotExisted_ReturnsNotFound()
    {
        const int id = 1;
        _categoryRepository.Setup(cs => cs.GetByIdAsync(1))
            .ReturnsAsync((Category?)null);

        var result = await _categoriesController.Delete(id);

        Assert.That(result, Is.TypeOf<NotFoundResult>());
    }

    [Test]
    public async Task Delete_ExistedCategory_ReturnsNoContent()
    {
        var category = new Category { Id = 1, Name = "something" };
        _categoryRepository.Setup(cs => cs.GetByIdAsync(category.Id))
            .ReturnsAsync(category);

        var result = await _categoriesController.Delete(category.Id);

        _categoryRepository.Verify(cr => cr.DeleteAsync(category),
            Times.Once());
        _unitOfWork.Verify(cr => cr.CompleteAsync(),
            Times.Once);
        Assert.That(result, Is.TypeOf<NoContentResult>());
    }
}