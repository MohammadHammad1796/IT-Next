using IT_Next.Core.Services;
using System.Text.Json;

namespace IT_Next.Infrastructure.Services;

public class JsonFileManager<TFileType> : IJsonFileManager<TFileType>
{
    private readonly string _filepath;

    public JsonFileManager(string filePath)
    {
        _filepath = filePath;
    }

    public TFileType? Get()
    {
        var text = File.ReadAllText(_filepath);
        return JsonSerializer.Deserialize<TFileType>(text);
    }

    public void Save(TFileType input)
    {
        var settings = JsonSerializer.Serialize(input, new JsonSerializerOptions()
        {
            WriteIndented = true,
        });

        File.WriteAllText(_filepath, settings);
    }
}