namespace IT_Next.Core.Services;

public interface ICertificateService
{
    Task<DateTime?> GetExpirationTimeAsync(string url);
    Task<DateTime?> GetIssueTimeAsync(string url);
}