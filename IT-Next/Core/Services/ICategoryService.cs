namespace IT_Next.Core.Services;

public interface ICategoryService
{
    Task<bool> IsUnique(string name, int id);
}