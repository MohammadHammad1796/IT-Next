using IT_Next.Controllers.UIs.Base;
using Microsoft.AspNetCore.Mvc;

namespace IT_Next.Controllers.UIs.Dashboard;

[Route("dashboard")]
public class DashboardController : DashboardUIController
{
    [Route("")]
    public IActionResult Index() => View();

    [Route("categories")]
    public IActionResult Categories() => View();

    [Route("subCategories")]
    public IActionResult SubCategories() => View();

    [Route("brands")]
    public IActionResult Brands() => View();

    [Route("products")]
    public IActionResult Products() => View();

    [Route("contactMessages")]
    public IActionResult ContactMessages() => View();
}