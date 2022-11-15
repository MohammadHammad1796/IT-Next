namespace IT_Next.Controllers.UIs.ViewModels;

public class ProductDetailsViewModel
{
    public string Name { get; set; }

    public string ImagePath { get; set; }

    public float Price { get; set; }

    public float Discount { get; set; }

    public int Quantity { get; set; }

    public string Description { get; set; }

    public DateTime LastUpdate { get; set; }

    public string SubCategoryName { get; set; }

    public string CategoryName { get; set; }

    public string BrandName { get; set; }
}