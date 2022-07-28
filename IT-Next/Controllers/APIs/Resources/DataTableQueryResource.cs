using IT_Next.Custom.ValidationAttributes;
using System.ComponentModel.DataAnnotations;

namespace IT_Next.Controllers.APIs.Resources;

public class DataTableQueryResource
{
    public bool IsSortAscending { get; set; }

    public virtual string SortBy { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "{0} should be at least {1}")]
    public int PageNumber { get; set; }

    [Range(10, 100, ErrorMessage = "{0} should be between {1} and {2}")]
    public int PageSize { get; set; }

    [StringNullableOrMinimumLength(2)]
    [StringNullableOrMaximumLength(50)]
    public virtual string? SearchQuery { get; set; }
}