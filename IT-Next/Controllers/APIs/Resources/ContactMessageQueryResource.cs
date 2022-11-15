using IT_Next.Core.Entities;
using IT_Next.Custom.ValidationAttributes;

namespace IT_Next.Controllers.APIs.Resources;

public class ContactMessageQueryResource : DataTableQueryResource
{
    [StringAllowedValues(typeof(ContactMessage))]
    public override string SortBy { get; set; }
}