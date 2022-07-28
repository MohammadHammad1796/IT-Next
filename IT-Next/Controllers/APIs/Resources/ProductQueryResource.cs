using IT_Next.Custom.ValidationAttributes;

namespace IT_Next.Controllers.APIs.Resources;

public class ProductQueryResource : DataTableQueryResource
{
    [StringAllowedValues(typeof(ProductResource), "ImagePath")]
    public override string SortBy { get; set; }

    [StringNullableOrMinimumLength(1)]
    [StringNullableOrMaximumLength(50)]
    public override string? SearchQuery { get; set; }
}