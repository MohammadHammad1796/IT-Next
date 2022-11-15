using System.ComponentModel.DataAnnotations;

namespace IT_Next.Core.Entities;

public class ContactMessage : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string CustomerName { get; set; }

    [Required]
    [MaxLength(100)]
    public string Email { get; set; }

    [Required]
    [MaxLength(10)]
    public string MobileNumber { get; set; }

    [Required]
    [MaxLength(250)]
    public string Message { get; set; }

    public DateTime Time { get; set; }

    public bool IsRead { get; set; }
}