using IT_Next.Infrastructure.Repositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Moq;
using NUnit.Framework;
using System.Threading.Tasks;

namespace IT_Next.UnitTests.Infrastructure.Repositories;

[TestFixture]
internal class FileRepositoryTests
{
    private Mock<IWebHostEnvironment> _environment;
    private Mock<IFormFile> _file;
    private FileRepository _repository;

    [SetUp]
    public void SetUp()
    {
        _environment = new Mock<IWebHostEnvironment>();
        _environment.Setup(e => e.WebRootPath).Returns("D:\\wwwroot");
        _file = new Mock<IFormFile>();
        _file.Setup(f => f.FileName).Returns("name.pdf");
        _repository = new FileRepository(_environment.Object);
    }

    [Test]
    public async Task SaveAsync_WhenSavedSuccessfully_ReturnsFileNameWithPath()
    {
        var result = await _repository.SaveAsync(_file.Object, "Documents");

        Assert.That(result!.Contains(".pdf"), Is.True);
        Assert.That(result.Contains("Documents"), Is.True);
    }

    [Test]
    public async Task SaveAsync_WhenSavedFail_ReturnsNull()
    {
        var result = await _repository.SaveAsync(_file.Object, null!);

        Assert.That(result, Is.Null);
    }

    [Test]
    public void Delete_WhenFileDeletedSuccessfully_ReturnsTrue()
    {
        var result = _repository.Delete("Documents/8bb86e08-f3a1-4f09-9168-a92d83e4595f.pdf");

        Assert.That(result, Is.True);
    }

    [Test]
    public void Delete_WhenDeleteFail_ReturnsFalse()
    {
        var result = _repository.Delete("path/name.extension");

        Assert.That(result, Is.False);
    }
}