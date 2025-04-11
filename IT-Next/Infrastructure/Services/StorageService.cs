using IT_Next.Core.Helpers;
using IT_Next.Core.Services;
using Microsoft.Extensions.Options;

namespace IT_Next.Infrastructure.Services;

public class StorageService : IStorageService
{
    private readonly string _rootPath;
    private readonly AppSetting _appSetting;

    public StorageService(IWebHostEnvironment environment, IOptions<AppSetting> options)
    {
        _rootPath = environment.ContentRootPath;
        _appSetting = options.Value;
    }

    public float GetUsedStorageInMB()
    {
        var directoryInfo = new DirectoryInfo(_rootPath);
        var directorySizeInBit = directoryInfo
            .EnumerateFiles("*", SearchOption.AllDirectories)
            .Sum(fileInfo => fileInfo.Length);
        var directorySizeInKiloByte = (float)directorySizeInBit / 1024;
        var directorySizeInMegaByte = directorySizeInKiloByte / 1024;
        return directorySizeInMegaByte;
    }

    public float GetAllStorageInMB()
    {
        var allSpace = _appSetting.HostingSpaceInMb;
        return allSpace;
    }
}