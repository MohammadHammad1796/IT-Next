using IT_Next.Core.Entities;
using IT_Next.Core.Repositories;
using IT_Next.Infrastructure.Services;
using Moq;
using NUnit.Framework;
using System.Threading.Tasks;

namespace IT_Next.UnitTests.Infrastructure.Services;

[TestFixture]
internal class BrandServiceTests
{
    private Mock<IBrandRepository> _brandRepository;
    private BrandService _brandService;

    [SetUp]
    public void Setup()
    {
        _brandRepository = new Mock<IBrandRepository>();
        _brandService = new BrandService(_brandRepository.Object);
    }

    [Test]
    public async Task IsUnique_BrandWithSameNameAndAnotherIdExisted_ReturnsFalse()
    {
        const string name = "something";
        const int id = 1;
        _brandRepository.Setup(cr => cr.GetByNameAsync(name))
            .ReturnsAsync(new Brand());

        var result = await _brandService.IsUniqueAsync(name, id);

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task IsUnique_BrandWithSameNameAndSameIdExisted_ReturnsTrue()
    {
        const string name = "something";
        const int id = 1;
        _brandRepository.Setup(cr => cr.GetByNameAsync(name))
            .ReturnsAsync(new Brand { Id = id });

        var result = await _brandService.IsUniqueAsync(name, id);

        Assert.That(result, Is.True);
    }

    [Test]
    public async Task IsUnique_BrandWithSameNameDoesNotExisted_ReturnsTrue()
    {
        const string name = "something";
        const int id = 1;
        _brandRepository.Setup(cr => cr.GetByNameAsync(name))
            .ReturnsAsync((Brand?)null);

        var result = await _brandService.IsUniqueAsync(name, id);

        Assert.That(result, Is.True);
    }
}