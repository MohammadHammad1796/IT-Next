using IT_Next.Controllers.UIs.BaseControllers;
using Microsoft.AspNetCore.Mvc;

namespace IT_Next.Controllers.UIs;

[Route("dashboard")]
public class DashboardController : DashboardUIController
{
    public IActionResult Index()
    {
        return View();
    }
}