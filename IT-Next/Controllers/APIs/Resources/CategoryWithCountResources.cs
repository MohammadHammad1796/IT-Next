namespace IT_Next.Controllers.APIs.Resources;

public class CategoryWithCountResources
{
    public int Count { get; set; }

    public ICollection<CategoryResource> Categories { get; set; }

    public CategoryWithCountResources(int count, ICollection<CategoryResource> resources)
    {
        Count = count;
        Categories = resources;
    }
}