using IT_Next.Custom.ValidationAttributes;

namespace IT_Next.Controllers.APIs.Resources;

public class SubCategoryProductsQueryResource : DataTableQueryResource
{
    [StringAllowedValues(typeof(ProductInSubCategoryProductsListResource), "ImagePath", "OldPrice", "NewPrice")]
    public override string SortBy { get; set; }

    [StringNullableOrMinimumLength(1)]
    [StringNullableOrMaximumLength(50)]
    public override string? SearchQuery { get; set; }

    public float? MinimumPrice { get; set; }

    public float? MaximumPrice { get; set; }

    public ICollection<int> BrandsIds { get; set; }

    public SubCategoryProductsQueryResource()
    {
        BrandsIds = new List<int>();
    }
}