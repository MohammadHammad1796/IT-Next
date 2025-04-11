using IT_Next.Core.Helpers;
using IT_Next.Core.Services;

namespace IT_Next.Infrastructure.Services;

public class SettingsManager : JsonFileManager<AppSetting>, ISettingsManager
{
    public SettingsManager(string filePath) : base(filePath)
    {
    }
}