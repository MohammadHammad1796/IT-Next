using System.Text.Json.Serialization;

namespace IT_Next.Controllers.APIs.Resources;

public class ProductInSubCategoryProductsListResource
{
    [JsonIgnore]
    public int Id { get; set; }

    [JsonIgnore]
    public int Price { get; set; }

    public string Name { get; set; }

    public string Brand { get; set; }

    public float OldPrice { get; set; }

    public float? NewPrice { get; set; }

    public string ImagePath { get; set; }
}