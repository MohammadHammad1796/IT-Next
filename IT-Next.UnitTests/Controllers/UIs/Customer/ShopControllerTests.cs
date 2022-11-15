using AutoMapper;
using IT_Next.Controllers.UIs.Customer;
using IT_Next.Core.Entities;
using IT_Next.Core.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Moq;
using NUnit.Framework;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace IT_Next.UnitTests.Controllers.UIs.Customer;

internal class ShopControllerTests
{
    private Mock<ISubCategoryRepository> _subCategoryRepository;
    private Mock<IProductRepository> _productRepository;
    private Mock<IMapper> _mapper;

    private ShopController _shopController;

    [SetUp]
    public void SetUp()
    {
        _subCategoryRepository = new Mock<ISubCategoryRepository>();
        _productRepository = new Mock<IProductRepository>();
        _mapper = new Mock<IMapper>();

        _shopController = new ShopController(_subCategoryRepository.Object,
            _productRepository.Object, _mapper.Object);
        _shopController.ControllerContext = new ControllerContext(
            new ActionContext(new DefaultHttpContext(),
                new RouteData(),
                new ControllerActionDescriptor()));
    }

    [Test]
    public async Task ListProducts_WhenSubCategoryDoesNotExisted_ReturnsNotFound()
    {
        const string subCategoryName = "test";
        _subCategoryRepository
            .Setup(scr => scr.GetByNameAsync(subCategoryName, It.IsAny<Expression<Func<SubCategory, object>>>()))
            .ReturnsAsync((SubCategory?)null);

        var result = await _shopController.ListProducts(It.IsAny<string>(), subCategoryName);

        Assert.That(result, Is.TypeOf<ViewResult>());
        Assert.That(_shopController.Response.StatusCode, Is.EqualTo(StatusCodes.Status404NotFound));
    }

    [Test]
    public async Task ListProducts_WhenSubCategoryExistedButWithDifferentCategoryName_ReturnsNotFound()
    {
        var subCategory = new SubCategory { Name = "test", Category = new Category { Name = "test2" } };
        _subCategoryRepository
            .Setup(scr => scr.GetByNameAsync(subCategory.Name, It.IsAny<Expression<Func<SubCategory, object>>>()))
            .ReturnsAsync(subCategory);

        var result = await _shopController.ListProducts("test1", subCategory.Name);

        Assert.That(result, Is.TypeOf<ViewResult>());
        Assert.That(_shopController.Response.StatusCode, Is.EqualTo(StatusCodes.Status404NotFound));
    }

    [Test]
    public async Task ListProducts_WhenSubCategoryExistedWithSameCategoryName_ReturnsViewResultWithOkStatus()
    {
        var subCategory = new SubCategory { Name = "test", Category = new Category { Name = "test" } };
        _subCategoryRepository
            .Setup(scr => scr.GetByNameAsync(subCategory.Name, It.IsAny<Expression<Func<SubCategory, object>>>()))
            .ReturnsAsync(subCategory);

        var result = await _shopController.ListProducts(subCategory.Category.Name, subCategory.Name);

        Assert.That(result, Is.TypeOf<ViewResult>());
        Assert.That(_shopController.Response.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
    }

    [Test]
    public async Task GetProduct_WhenProductDoesNotExisted_ReturnsNotFound()
    {
        const string productName = "test";
        _productRepository
            .Setup(pr => pr.GetByNameAsync(productName, It.IsAny<Expression<Func<Product, object>>[]>()))
            .ReturnsAsync((Product?)null);

        var result = await _shopController.GetProduct(It.IsAny<string>(), It.IsAny<string>(), productName);

        Assert.That(result, Is.TypeOf<ViewResult>());
        Assert.That(_shopController.Response.StatusCode, Is.EqualTo(StatusCodes.Status404NotFound));
    }

    [Test]
    public async Task GetProduct_WhenProductExistedButWithDifferentSubCategoryName_ReturnsNotFound()
    {
        var product = new Product() { Name = "test", SubCategory = new SubCategory() { Name = "test2" } };
        _productRepository
            .Setup(pr => pr.GetByNameAsync(product.Name, It.IsAny<Expression<Func<Product, object>>[]>()))
            .ReturnsAsync(product);

        var result = await _shopController.GetProduct(It.IsAny<string>(), "test1", product.Name);

        Assert.That(result, Is.TypeOf<ViewResult>());
        Assert.That(_shopController.Response.StatusCode, Is.EqualTo(StatusCodes.Status404NotFound));
    }

    [Test]
    public async Task GetProduct_WhenProductExistedButWithDifferentCategoryName_ReturnsNotFound()
    {
        var product = new Product()
        {
            Name = "test",
            SubCategory = new SubCategory()
            {
                Name = "test",
                Category = new Category() { Name = "test2" }
            }
        };
        _productRepository
            .Setup(pr => pr.GetByNameAsync(product.Name, It.IsAny<Expression<Func<Product, object>>[]>()))
            .ReturnsAsync(product);

        var result = await _shopController.GetProduct("test1", product.SubCategory.Name, product.Name);

        Assert.That(result, Is.TypeOf<ViewResult>());
        Assert.That(_shopController.Response.StatusCode, Is.EqualTo(StatusCodes.Status404NotFound));
    }

    [Test]
    public async Task GetProduct_WhenProductExistedWithSameSubCategoryNameAndCategoryName_ReturnsViewResultWithOkStatus()
    {
        var product = new Product()
        {
            Name = "test",
            SubCategory = new SubCategory()
            {
                Name = "test",
                Category = new Category() { Name = "test" }
            },
            Brand = new Brand()
        };
        _productRepository
            .Setup(pr => pr.GetByNameAsync(product.Name, It.IsAny<Expression<Func<Product, object>>[]>()))
            .ReturnsAsync(product);

        var result = await _shopController.GetProduct(product.SubCategory.Category.Name, product.SubCategory.Name, product.Name);

        Assert.That(result, Is.TypeOf<ViewResult>());
        Assert.That(_shopController.Response.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
    }
}