using System.ComponentModel.DataAnnotations;

namespace IT_Next.Core.Helpers;

public class AppSetting
{
    [Required]
    [Display(Name = "Https port")]
    [Range(1000, int.MaxValue, ErrorMessage = "{0} should be at least {1}")]
    public int HttpsPort { get; set; }

    [Required]
    [Display(Name = "Connection string")]
    public string ConnectionString { get; set; }

    [Display(Name = "Hosting space in Mega byte")]
    [Range(100, int.MaxValue, ErrorMessage = "{0} should be at least {1} Mega byte")]
    public int HostingSpaceInMb { get; set; }
}