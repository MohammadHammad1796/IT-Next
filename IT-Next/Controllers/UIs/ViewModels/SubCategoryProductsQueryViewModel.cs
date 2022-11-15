namespace IT_Next.Controllers.UIs.ViewModels;

public class SubCategoryProductsQueryViewModel
{
    public string? NameQuery { get; set; }

    public string? SortBy { get; set; }

    public bool IsSortAscending { get; set; }

    public int PageNumber { get; set; }

    public int PageSize { get; set; }

    public float MinimumPrice { get; set; }

    public float MaximumPrice { get; set; }

    public ICollection<int> BrandsIds { get; set; }

    public SubCategoryProductsQueryViewModel()
    {
        BrandsIds = new List<int>();
    }
}