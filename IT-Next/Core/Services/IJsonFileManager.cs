namespace IT_Next.Core.Services;

public interface IJsonFileManager<TFileType>
{
    TFileType? Get();
    void Save(TFileType input);
}