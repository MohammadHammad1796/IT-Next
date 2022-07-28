namespace IT_Next.Controllers.APIs.Resources;

public class SubCategoryResource
{
    public int Id { get; set; }

    public string Name { get; set; }

    public int CategoryId { get; set; }

    public string CategoryName { get; set; }
}