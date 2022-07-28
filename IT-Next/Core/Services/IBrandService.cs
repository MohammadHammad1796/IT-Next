namespace IT_Next.Core.Services;

public interface IBrandService
{
    Task<bool> IsUniqueAsync(string name, int id);
}