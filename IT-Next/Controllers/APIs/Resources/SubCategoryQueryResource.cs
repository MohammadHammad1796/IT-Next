using IT_Next.Custom.ValidationAttributes;

namespace IT_Next.Controllers.APIs.Resources;

public class SubCategoryQueryResource : DataTableQueryResource
{
    [StringAllowedValues(typeof(SubCategoryResource))]
    public override string SortBy { get; set; }

    public bool IncludeCategory { get; set; }

    public bool SearchByCategory { get; set; }
}