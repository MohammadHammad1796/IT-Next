using System.ComponentModel.DataAnnotations;

namespace IT_Next.Core.Entities;

public class Brand : EntityWithUniqueName
{
    [Required]
    [MaxLength(50)]
    public override string Name { get; set; }

    public ICollection<Product> Products { get; set; }

    public Brand()
    {
        Products = new List<Product>();
    }
}