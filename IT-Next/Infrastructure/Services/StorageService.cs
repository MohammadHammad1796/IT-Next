using IT_Next.Core.Services;

namespace IT_Next.Infrastructure.Services;

public class StorageService : IStorageService
{
    private readonly string _rootPath;
    private readonly IConfiguration _configuration;

    public StorageService(IWebHostEnvironment environment, IConfiguration configuration)
    {
        _rootPath = environment.WebRootPath;
        _configuration = configuration;
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
        var allSpace = float.Parse(_configuration["Hosting:Space"]);
        return allSpace;
    }
}