using IT_Next.Custom.ValidationAttributes;

namespace IT_Next.Controllers.APIs.Resources;

public class BrandQueryResource : DataTableQueryResource
{
    [StringAllowedValues(typeof(BrandResource))]
    public override string SortBy { get; set; }
}