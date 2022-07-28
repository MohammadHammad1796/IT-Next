using IT_Next.Core.Entities;
using IT_Next.Core.Repositories;
using IT_Next.Infrastructure.Services;
using Moq;
using NUnit.Framework;
using System.Threading.Tasks;

namespace IT_Next.UnitTests.Infrastructure.Services;

[TestFixture]
internal class CategoryServiceTests
{
    private Mock<ICategoryRepository> _categoryRepository;
    private CategoryService _categoryService;

    [SetUp]
    public void Setup()
    {
        _categoryRepository = new Mock<ICategoryRepository>();
        _categoryService = new CategoryService(_categoryRepository.Object);
    }

    [Test]
    public async Task IsUnique_CategoryWithSameNameAndAnotherIdExisted_ReturnsFalse()
    {
        const string name = "something";
        const int id = 1;
        _categoryRepository.Setup(cr => cr.GetByNameAsync(name))
            .ReturnsAsync(new Category());

        var result = await _categoryService.IsUnique(name, id);

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task IsUnique_CategoryWithSameNameAndSameIdExisted_ReturnsTrue()
    {
        const string name = "something";
        const int id = 1;
        _categoryRepository.Setup(cr => cr.GetByNameAsync(name))
            .ReturnsAsync(new Category { Id = id });

        var result = await _categoryService.IsUnique(name, id);

        Assert.That(result, Is.True);
    }

    [Test]
    public async Task IsUnique_CategoryWithSameNameDoesNotExisted_ReturnsTrue()
    {
        const string name = "something";
        const int id = 1;
        _categoryRepository.Setup(cr => cr.GetByNameAsync(name))
            .ReturnsAsync((Category?)null);

        var result = await _categoryService.IsUnique(name, id);

        Assert.That(result, Is.True);
    }
}