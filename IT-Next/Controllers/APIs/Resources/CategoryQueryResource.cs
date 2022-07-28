using IT_Next.Custom.ValidationAttributes;

namespace IT_Next.Controllers.APIs.Resources;

public class CategoryQueryResource : DataTableQueryResource
{
    [StringAllowedValues(typeof(CategoryResource))]
    public override string SortBy { get; set; }
}