using Microsoft.AspNetCore.Mvc;

namespace IT_Next.Controllers.UIs.BaseControllers;

[ApiExplorerSettings(IgnoreApi = true)]
// ReSharper disable once InconsistentNaming
public abstract class DashboardUIController : BaseUIController
{
    protected DashboardUIController() : base("dashboard")
    {
    }
}