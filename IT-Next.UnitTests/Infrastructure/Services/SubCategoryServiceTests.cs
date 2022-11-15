using IT_Next.Core.Entities;
using IT_Next.Core.Repositories;
using IT_Next.Infrastructure.Services;
using Moq;
using NUnit.Framework;
using System.Threading.Tasks;

namespace IT_Next.UnitTests.Infrastructure.Services;

[TestFixture]
internal class SubCategoryServiceTests
{
    private Mock<ISubCategoryRepository> _subCategoryRepository;
    private Mock<ICategoryRepository> _categoryRepository;
    private SubCategoryService _subCategoryService;

    [SetUp]
    public void Setup()
    {
        _subCategoryRepository = new Mock<ISubCategoryRepository>();
        _categoryRepository = new Mock<ICategoryRepository>();
        _subCategoryService = new SubCategoryService(_subCategoryRepository.Object, 
            _categoryRepository.Object);
    }

    [Test]
    public async Task IsUnique_SubCategoryWithSameNameAndAnotherIdExisted_ReturnsFalse()
    {
        const string name = "something";
        const int id = 1;
        _subCategoryRepository.Setup(scr => scr.GetByNameAsync(name))
            .ReturnsAsync(new SubCategory());

        var result = await _subCategoryService.IsUniqueAsync(name, id);

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task IsUnique_SubCategoryWithSameNameSameIdExisted_ReturnsTrue()
    {
        const string name = "something";
        const int id = 1;
        _subCategoryRepository.Setup(scr => scr.GetByNameAsync(name))
            .ReturnsAsync(new SubCategory { Id = id });

        var result = await _subCategoryService.IsUniqueAsync(name, id);

        Assert.That(result, Is.True);
    }

    [Test]
    public async Task IsUnique_SubCategoryWithSameNameAndSameIdDoesNotExisted_ReturnsTrue()
    {
        const string name = "something";
        const int id = 1;
        _subCategoryRepository.Setup(scr => scr.GetByNameAsync(name))
            .ReturnsAsync((SubCategory?)null);

        var result = await _subCategoryService.IsUniqueAsync(name, id);

        Assert.That(result, Is.True);
    }
}