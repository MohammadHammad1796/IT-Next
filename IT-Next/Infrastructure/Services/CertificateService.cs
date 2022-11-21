using IT_Next.Core.Services;

namespace IT_Next.Infrastructure.Services;

public class CertificateService : ICertificateService
{
    public async Task<DateTime?> GetIssueTimeAsync(string url)
    {
        using var handler = new HttpClientHandler();
        DateTime? issueTime = null;
        handler.ServerCertificateCustomValidationCallback = (requestMessage, certificate, x509Chain, SslPolicyErrors) =>
        {
            if (certificate is null)
                return false;

            issueTime = DateTime.Parse(certificate.GetEffectiveDateString());
            return true;
        };

        using var client = new HttpClient(handler);

        using var response = await client.GetAsync(url);
        return issueTime;
    }

    public async Task<DateTime?> GetExpirationTimeAsync(string url)
    {
        using var handler = new HttpClientHandler();
        DateTime? expirationTime = null;
        handler.ServerCertificateCustomValidationCallback = (requestMessage, certificate, x509Chain, SslPolicyErrors) =>
        {
            if (certificate is null)
                return false;

            expirationTime = DateTime.Parse(certificate.GetExpirationDateString());
            return true;
        };

        using var client = new HttpClient(handler);

        using var response = await client.GetAsync(url);
        return expirationTime;
    }
}