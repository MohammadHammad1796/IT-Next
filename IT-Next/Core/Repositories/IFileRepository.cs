namespace IT_Next.Core.Repositories;

public interface IFileRepository
{
    Task<string?> SaveAsync(IFormFile file, string folderPath);

    bool Delete(string fileNameWithPath);
}