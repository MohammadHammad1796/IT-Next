namespace IT_Next.Controllers.APIs.Resources;

public class CertificateInfoResource
{
    public DateTime IssueTime { get; }
    public DateTime ExpirationTime { get; }

    public CertificateInfoResource(DateTime issueTime, DateTime expirationTime)
    {
        IssueTime = issueTime;
        ExpirationTime = expirationTime;
    }
}