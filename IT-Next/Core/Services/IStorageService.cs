namespace IT_Next.Core.Services;

public interface IStorageService
{
    float GetUsedStorageInMB();

    float GetAllStorageInMB();
}