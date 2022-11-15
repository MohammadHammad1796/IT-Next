using System.ComponentModel.DataAnnotations;

namespace IT_Next.Controllers.SharedModels;

public class SaveSubCategorySharedModel
{
    public int Id { get; set; }

    [MaxLength(100, ErrorMessage = "The {0} field maximum length is {1}.")]
    [MinLength(2, ErrorMessage = "The {0} field minimum length is {1}.")]
    public string Name { get; set; }

    public int CategoryId { get; set; }
}