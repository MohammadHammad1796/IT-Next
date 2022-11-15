using Microsoft.AspNetCore.Mvc;

namespace IT_Next.Controllers.UIs.Base;

[ApiExplorerSettings(IgnoreApi = true)]
// ReSharper disable once InconsistentNaming
public class ClientUIController : BaseUIController
{
    public ClientUIController() : base(ApplicationLayout._CustomerLayout)
    {
    }
}