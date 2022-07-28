using Microsoft.AspNetCore.Http;

namespace IT_Next.UnitTests.Custom.ValidationAttributes;

internal class TestValidationModel
{
    public int Id { get; set; }

    public string Name { get; set; }

    public double Price { get; set; }

    public IFormFile? FormFile { get; set; }
}