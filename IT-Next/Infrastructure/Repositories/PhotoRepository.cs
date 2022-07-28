using IT_Next.Core.Repositories;
using static System.IO.Path;

namespace IT_Next.Infrastructure.Repositories;

public class PhotoRepository : FileRepository, IPhotoRepository
{
    private const string ImagesFolderName = "images";

    public PhotoRepository(IHostEnvironment host) : base(host)
    {
    }

    public override Task<string?> SaveAsync(IFormFile file, string folderPath)
    {
        folderPath = Combine(ImagesFolderName, folderPath);
        return base.SaveAsync(file, folderPath);
    }
}