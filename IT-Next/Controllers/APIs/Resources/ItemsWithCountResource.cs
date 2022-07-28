namespace IT_Next.Controllers.APIs.Resources;

public class ItemsWithCountResource<TResource> where TResource : class
{
    public int Total { get; set; }

    public ICollection<TResource> Items { get; set; }

    public ItemsWithCountResource(int total, ICollection<TResource> resources)
    {
        Total = total;
        Items = resources;
    }
}