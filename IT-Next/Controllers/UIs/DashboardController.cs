using IT_Next.Controllers.UIs.BaseControllers;
using Microsoft.AspNetCore.Mvc;

namespace IT_Next.Controllers.UIs;

[Route("dashboard")]
public class DashboardController : DashboardUIController
{
    [Route("index")]
    public IActionResult Index() => View();

    [Route("categories")]
    public IActionResult Categories() => View();

    [Route("subCategories")]
    public IActionResult SubCategories() => View();

    [Route("brands")]
    public IActionResult Brands() => View();

    [Route("products")]
    public IActionResult Products() => View();
}