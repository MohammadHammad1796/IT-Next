namespace IT_Next.Controllers.APIs.Resources;

public class StorageInfoResource
{
    public float All { get; }
    public float Used { get; }

    public StorageInfoResource(float all, float used)
    {
        All = all;
        Used = used;
    }
}