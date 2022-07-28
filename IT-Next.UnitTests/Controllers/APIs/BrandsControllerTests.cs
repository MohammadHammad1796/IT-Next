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
internal class BrandsControllerTests
{
    private BrandsController _brandsController;
    private Mock<IMapper> _mapper;
    private Mock<IUnitOfWork> _unitOfWork;
    private Mock<IBrandRepository> _brandRepository;
    private Mock<IBrandService> _brandService;

    [SetUp]
    public void Setup()
    {
        _brandRepository = new Mock<IBrandRepository>();
        _brandService = new Mock<IBrandService>();
        _unitOfWork = new Mock<IUnitOfWork>();
        _mapper = new Mock<IMapper>();

        _brandsController = new BrandsController(_mapper.Object, _unitOfWork.Object,
            _brandRepository.Object, _brandService.Object);
    }

    [Test]
    public async Task Get_WhenTotalOrdered_ReturnsOkResultContainsBrandResourcesWithTotals()
    {
        var brands = new List<Brand> { new(), new() };
        var brandResources = new List<BrandResource> { new(), new() };
        var brandQuery = new Query<Brand>();
        _brandRepository.Setup(cr => cr.GetAsync(brandQuery))
            .ReturnsAsync(brands);
        _brandRepository.Setup(cr => cr.GetCountAsync(brandQuery.Conditions))
            .ReturnsAsync(brands.Count);
        var queryResource = new BrandQueryResource();
        _mapper.Setup(m => m.Map<Query<Brand>>(queryResource))
            .Returns(brandQuery);
        _mapper.Setup(m => m.Map<IEnumerable<BrandResource>>(brands))
            .Returns(brandResources);

        var result = await _brandsController.Get(queryResource);

        Assert.That(result, Is.TypeOf<OkObjectResult>());
        var objectResult = result as OkObjectResult;
        Assert.That(objectResult, Is.Not.Null);

        Assert.That(objectResult!.Value, Is.TypeOf<ItemsWithCountResource<BrandResource>>());
        var resources = objectResult.Value as ItemsWithCountResource<BrandResource>;
        Assert.That(resources!.Total, Is.EqualTo(2));
        Assert.That(resources.Items.Count, Is.EqualTo(2));
    }

    [Test]
    public async Task Get_WhenTotalNotOrdered_ReturnsOkResultContainsBrandResources()
    {
        var brands = new List<Brand> { new(), new() };
        var brandResources = new List<BrandResource> { new(), new() };
        var brandQuery = new Query<Brand>();
        _brandRepository.Setup(cr => cr.GetAsync(brandQuery))
            .ReturnsAsync(brands);
        _brandRepository.Setup(cr => cr.GetCountAsync(brandQuery.Conditions))
            .ReturnsAsync(brands.Count);
        var queryResource = new BrandQueryResource();
        _mapper.Setup(m => m.Map<Query<Brand>>(queryResource))
            .Returns(brandQuery);
        _mapper.Setup(m => m.Map<IEnumerable<BrandResource>>(brands))
            .Returns(brandResources);

        var result = await _brandsController.Get(queryResource, withTotal: false);

        Assert.That(result, Is.TypeOf<OkObjectResult>());
        var objectResult = result as OkObjectResult;
        Assert.That(objectResult, Is.Not.Null);

        Assert.That(objectResult!.Value, Is.TypeOf<List<BrandResource>>());
        var resources = objectResult.Value as IEnumerable<BrandResource>;
        Assert.That(resources!.Count(), Is.EqualTo(2));
    }

    [Test]
    public async Task GetById_BrandExists_ReturnsOkResultContainsBrandResource()
    {
        var brand = new Brand { Id = 1, Name = "something" };
        _brandRepository.Setup(cr => cr.GetByIdAsync(1))
            .ReturnsAsync(brand);
        var brandResource = new BrandResource { Id = 1, Name = "something" };
        _mapper.Setup(m => m.Map<BrandResource>(brand))
            .Returns(brandResource);

        var result = await _brandsController.GetById(1);

        Assert.That(result, Is.TypeOf<OkObjectResult>());
        var objectResult = result as OkObjectResult;
        Assert.That(objectResult, Is.Not.Null);

        Assert.That(objectResult?.Value, Is.EqualTo(brandResource));
    }

    [Test]
    public async Task GetById_BrandDoesNotExists_ReturnsNotFoundResult()
    {
        _brandRepository.Setup(cr => cr.GetByIdAsync(0))
            .ReturnsAsync((Brand?)null);

        var result = await _brandsController.GetById(1);

        Assert.That(result, Is.TypeOf<NotFoundResult>());
    }

    [Test]
    public async Task Save_UpdateNotExistedBrand_ReturnsNotFoundResult()
    {
        var brandResource = new BrandResource { Id = 1, Name = "something" };
        _brandRepository.Setup(cr => cr.GetByIdAsync(brandResource.Id))
            .ReturnsAsync((Brand?)null);

        var result = await _brandsController.Save(brandResource);

        Assert.That(result, Is.TypeOf<NotFoundResult>());
    }

    [Test]
    public async Task Save_CreateBrand_ReturnsOkResultContainsBrandResource()
    {
        var brandResource = new BrandResource { Id = 0, Name = "something" };
        var brand = new Brand { Id = brandResource.Id, Name = brandResource.Name };
        _brandService.Setup(cs => cs
                .IsUniqueAsync(brandResource.Name, brandResource.Id))
            .ReturnsAsync(true);
        _mapper.Setup(m => m.Map(brandResource, It.IsAny<Brand>()))
            .Returns(brand);
        _brandRepository.Setup(cr => cr.AddAsync(brand))
            .Callback(() => { brand.Id = 1; });
        _mapper.Setup(m => m.Map(It.IsAny<Brand>(), It.IsAny<BrandResource>()))
            .Returns(() =>
            {
                brandResource.Id = brand.Id;
                return brandResource;
            });

        var result = await _brandsController.Save(brandResource);

        _brandRepository.Verify(cr => cr.AddAsync(It.IsAny<Brand>()),
            Times.Once);
        _unitOfWork.Verify(cr => cr.CompleteAsync(),
            Times.Once);

        Assert.That(result, Is.TypeOf<OkObjectResult>());
        var objectResult = result as OkObjectResult;
        Assert.That(objectResult, Is.Not.Null);
        Assert.That(objectResult?.Value, Is.EqualTo(brandResource));
    }

    [Test]
    [TestCase(0)]
    [TestCase(1)]
    public async Task Save_CreateOrUpdateBrandWithDuplicatedName_ReturnsBadRequestResultWithModelErrors(int id)
    {
        var brandResource = new BrandResource { Id = id, Name = "something" };
        var brand = new Brand { Id = id, Name = brandResource.Name };
        _brandRepository.Setup(cr => cr.GetByIdAsync(1))
            .ReturnsAsync(brand);
        _brandService.Setup(cs => cs
                .IsUniqueAsync(brandResource.Name, brandResource.Id))
            .ReturnsAsync(false);

        var result = await _brandsController.Save(brandResource);

        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        var objectResult = result as BadRequestObjectResult;
        Assert.That(objectResult, Is.Not.Null);
        var modelState = objectResult?.Value as Dictionary<string, object>;
        Assert.That(modelState?.ContainsKey(nameof(brandResource.Name)), Is.True);
    }

    [Test]
    public async Task Save_UpdateBrand_ReturnsOkResultContainsBrandResource()
    {
        var brandResource = new BrandResource { Id = 1, Name = "something" };
        var brand = new Brand { Id = brandResource.Id, Name = brandResource.Name };
        _brandRepository.Setup(cr => cr.GetByIdAsync(brandResource.Id))
            .ReturnsAsync(brand);
        _brandService.Setup(cs => cs
                .IsUniqueAsync(brandResource.Name, brandResource.Id))
            .ReturnsAsync(true);
        _mapper.Setup(m => m.Map(brandResource, It.IsAny<Brand>()))
            .Returns(brand);
        _mapper.Setup(m => m.Map(It.IsAny<Brand>(), It.IsAny<BrandResource>()))
            .Returns(() =>
            {
                brandResource.Id = brand.Id;
                return brandResource;
            });

        var result = await _brandsController.Save(brandResource);

        _brandRepository.Verify(cr => cr.AddAsync(It.IsAny<Brand>()),
            Times.Never);

        Assert.That(result, Is.TypeOf<OkObjectResult>());
        var objectResult = result as OkObjectResult;
        Assert.That(objectResult, Is.Not.Null);
        Assert.That(objectResult?.Value, Is.EqualTo(brandResource));
    }

    [Test]
    public async Task Delete_BrandDoesNotExisted_ReturnsNotFound()
    {
        const int id = 1;
        _brandRepository.Setup(cs => cs.GetByIdAsync(1))
            .ReturnsAsync((Brand?)null);

        var result = await _brandsController.Delete(id);

        Assert.That(result, Is.TypeOf<NotFoundResult>());
    }

    [Test]
    public async Task Delete_ExistedBrand_ReturnsNoContent()
    {
        var brand = new Brand { Id = 1, Name = "something" };
        _brandRepository.Setup(cs => cs.GetByIdAsync(brand.Id))
            .ReturnsAsync(brand);

        var result = await _brandsController.Delete(brand.Id);

        _brandRepository.Verify(cr => cr.DeleteAsync(brand),
            Times.Once());
        _unitOfWork.Verify(cr => cr.CompleteAsync(),
            Times.Once);
        Assert.That(result, Is.TypeOf<NoContentResult>());
    }
}