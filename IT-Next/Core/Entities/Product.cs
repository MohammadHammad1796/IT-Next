using System.ComponentModel.DataAnnotations;

namespace IT_Next.Core.Entities;

public class Product : EntityWithUniqueName
{
    [Required]
    [MaxLength(100)]
    public override string Name { get; set; }

    [Required]
    public string ImagePath { get; set; }

    public float Price { get; set; }

    public float Discount { get; set; }

    public int Quantity { get; set; }

    [Required]
    [MaxLength(250)]
    public string Description { get; set; }

    public DateTime LastUpdate { get; set; }

    public int SubCategoryId { get; set; }

    public SubCategory SubCategory { get; set; }

    public int BrandId { get; set; }

    public Brand Brand { get; set; }

    public Product()
    {
    }

    // ReSharper disable once SuggestBaseTypeForParameterInConstructor
    public Product(Product product)
        : base(product)
    {
    }
}