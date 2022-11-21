using IT_Next.Custom.ValidationAttributes;
using System.ComponentModel.DataAnnotations;

namespace IT_Next.Controllers.SharedModels;

public class SaveProductSharedModel
{
    public int Id { get; set; }

    [MaxLength(100, ErrorMessage = "The {0} field maximum length is {1}.")]
    [MinLength(2, ErrorMessage = "The {0} field minimum length is {1}.")]
    [Required]
    public string Name { get; set; }

    [Range(0, float.MaxValue, ErrorMessage = "The field {0} minimum value is {1}.")]
    public float Price { get; set; }

    [Range(0, 0.99)]
    public float Discount { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "The field {0} minimum value is {1}.")]
    public int Quantity { get; set; }

    [Required]
    [MaxLength(250)]
    public string Description { get; set; }

    public int SubCategoryId { get; set; }

    public int BrandId { get; set; }

    [FileTypeImage]
    [RequiredIfIntegerIdZero]
    [SquareImage]
    [MinimumImageWidth(200)]
    public IFormFile? Image { get; set; }
}