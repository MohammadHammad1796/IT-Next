using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace IT_Next.Controllers.APIs.Resources;

public class CategoryQueryResource
{
    public bool IsSortAscending { get; set; } = true;

    [Range(1, int.MaxValue, ErrorMessage = "{0} should be at least {1}")]
    [DisplayName("Page number")]
    public int PageNumber { get; set; } = 1;

    [Range(10, 100, ErrorMessage = "{0} should be between {1} and {2}")]
    [DisplayName("Page size")]
    public int PageSize { get; set; } = 10;

    [MinLength(3)]
    public string? NameQuery { get; set; }
}