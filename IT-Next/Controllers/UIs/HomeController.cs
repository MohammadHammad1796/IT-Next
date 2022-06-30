using IT_Next.Controllers.UIs.BaseControllers;
using Microsoft.AspNetCore.Mvc;

namespace IT_Next.Controllers.UIs;

public class HomeController : NormalUIController
{
    public IActionResult Index()
    {
        return View();
    }
}