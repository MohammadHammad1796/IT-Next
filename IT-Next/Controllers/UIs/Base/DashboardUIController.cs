using Microsoft.AspNetCore.Mvc;

namespace IT_Next.Controllers.UIs.Base;

[ApiExplorerSettings(IgnoreApi = true)]
// ReSharper disable once InconsistentNaming
public abstract class DashboardUIController : BaseUIController
{
    protected DashboardUIController() : base(ApplicationLayout._DashboardLayout)
    {
    }
}