using Microsoft.AspNetCore.Mvc;

namespace IT_Next.Controllers.UIs.BaseControllers;

[ApiExplorerSettings(IgnoreApi = true)]
// ReSharper disable once InconsistentNaming
public class NormalUIController : BaseUIController
{
    public NormalUIController() : base("normal")
    {
    }
}