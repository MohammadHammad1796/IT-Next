namespace IT_Next.Controllers.APIs.Resources;

public class ProductResource
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string ImagePath { get; set; }

    public float Price { get; set; }

    public float Discount { get; set; }

    public int Quantity { get; set; }

    public string Description { get; set; }

    public DateTime LastUpdate { get; set; }

    public string SubCategoryName { get; set; }

    public int SubCategoryId { get; set; }

    public string CategoryName { get; set; }

    public string BrandName { get; set; }

    public int BrandId { get; set; }
}