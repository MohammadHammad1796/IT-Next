using Microsoft.AspNetCore.Mvc;

namespace IT_Next.Controllers.UIs.BaseControllers;

// ReSharper disable once InconsistentNaming
public abstract class BaseUIController : Controller
{
    private readonly string _layout;

    public BaseUIController(string layoutName)
    {
        _layout = layoutName;
    }

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
}