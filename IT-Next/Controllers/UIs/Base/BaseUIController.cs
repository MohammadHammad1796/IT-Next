using Microsoft.AspNetCore.Mvc;

namespace IT_Next.Controllers.UIs.Base;

// ReSharper disable once InconsistentNaming
public abstract class BaseUIController : Controller
{
    private readonly string _layout;
    /*protected ApplicationLayout CurrentLayout;*/

    protected BaseUIController(ApplicationLayout currentLayout) 
        => _layout = currentLayout.ToString();

    public override ViewResult View()
    {
        ViewBag.Layout = _layout;
        return base.View();
    }

    public override ViewResult View(string? viewName)
    {
        ViewBag.Layout = _layout;
        return base.View(viewName);
    }

    public override ViewResult View(object? model)
    {
        ViewBag.Layout = _layout;
        return base.View(model);
    }

    public override ViewResult View(string? viewName, object? model)
    {
        ViewBag.Layout = _layout;
        return base.View(viewName, model);
    }

    protected new IActionResult NotFound()
    {
        Response.StatusCode = StatusCodes.Status404NotFound;
        return View("NotFound");
    }
}