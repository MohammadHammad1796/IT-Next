using System.ComponentModel.DataAnnotations;

namespace IT_Next.Controllers.APIs.Resources;

public class BrandResource
{
    public int Id { get; set; }

    [MaxLength(50, ErrorMessage = "The {0} field maximum length is {1}.")]
    [MinLength(2, ErrorMessage = "The {0} field minimum length is {1}.")]
    public string Name { get; set; }
}