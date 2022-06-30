using System.ComponentModel.DataAnnotations;

namespace IT_Next.Core.Entities;

public class Category : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; }
}