using System.ComponentModel.DataAnnotations;

namespace IT_Next.Core.Entities;

public class SubCategory : EntityWithUniqueName
{
    [Required]
    [MaxLength(100)]
    public override string Name { get; set; }

    public int CategoryId { get; set; }

    public Category Category { get; set; }
}