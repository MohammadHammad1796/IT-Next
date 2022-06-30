using IT_Next.Controllers.UIs.BaseControllers;
using Microsoft.AspNetCore.Mvc;

namespace IT_Next.Controllers.UIs;

[Route("dashboard/categories")]
public class CategoriesController : DashboardUIController
{
    public IActionResult Index()
    {
        return View();
    }
}