namespace IT_Next.Core.Services;

public interface IDbManager
{
    Task<bool> TestConnectionStringAsync(string connectionString);
}