using IT_Next.Core.Repositories;
using static System.Guid;
using static System.IO.Directory;
using static System.IO.Path;

namespace IT_Next.Infrastructure.Repositories;

public class FileRepository : IFileRepository
{
    private readonly string _rootPath;

    public FileRepository(IWebHostEnvironment host)
    {
        _rootPath = host.WebRootPath;
    }

    public virtual async Task<string?> SaveAsync(IFormFile file, string folderPath)
    {
        string fileNameWithPath = null;
        try
        {
            var fullPath = Combine(_rootPath, folderPath);
            if (!Exists(fullPath))
                CreateDirectory(fullPath);

            var fileName = NewGuid() + GetExtension(file.FileName);
            var fullFilePath = Combine(fullPath, fileName);

            var stream = new FileStream(fullFilePath, FileMode.Create);
            await using var _ = stream.ConfigureAwait(false);
            await file.CopyToAsync(stream);

            fileNameWithPath = Combine(folderPath, fileName);
        }
        catch (Exception)
        { /* ignored */ }

        return fileNameWithPath;
    }

    public bool Delete(string fileNameWithPath)
    {
        try
        {
            var fullFilePath = Combine(_rootPath, fileNameWithPath);
            File.Delete(fullFilePath);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}