using System.ComponentModel.DataAnnotations;

namespace IT_Next.Controllers.SharedModels;

public class SendContactMessageSharedModel
{
    [Required]
    [MaxLength(100)]
    [Display(Name = "Name")]
    public string CustomerName { get; set; }

    [Required]
    [RegularExpression("^\\S+@\\S+\\.\\S+$", ErrorMessage = "{0} is not a valid email address")]
    public string Email { get; set; }

    [Required]
    [RegularExpression("^09[0-9]{8}$", ErrorMessage = "{0} is not a valid mobile number")]
    [Display(Name = "Mobile number")]
    public string MobileNumber { get; set; }

    [Required]
    [MaxLength(250)]
    public string Message { get; set; }
}