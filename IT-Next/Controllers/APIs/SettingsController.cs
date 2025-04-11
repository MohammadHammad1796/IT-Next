using IT_Next.Core.Helpers;
using IT_Next.Core.Services;
using IT_Next.Custom.RouteAttributes;
using Microsoft.AspNetCore.Mvc;

namespace IT_Next.Controllers.APIs;

[ApiRoute("settings")]
[ApiController]
public class SettingsController : Controller
{
    private readonly ISettingsManager _settingsManager;
    private readonly IDbManager _dbManager;

    public SettingsController(ISettingsManager settingsManager, IDbManager dbManager)
    {
        _settingsManager = settingsManager;
        _dbManager = dbManager;
    }

    public IActionResult Get()
    {
        return Ok(_settingsManager.Get());
    }

    [HttpPost]
    public async Task<IActionResult> Save(AppSetting settings)
    {
        var isConnectionStringValid = await _dbManager.TestConnectionStringAsync(settings.ConnectionString);
        if (!isConnectionStringValid)
        {
            ModelState.AddModelError(nameof(settings.ConnectionString), "Connection string is not valid");
            return BadRequest(ModelState);
        }

        _settingsManager.Save(settings);
        return Ok();
    }
}