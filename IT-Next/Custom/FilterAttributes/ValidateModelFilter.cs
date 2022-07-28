using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace IT_Next.Custom.FilterAttributes;

// ReSharper disable once ClassNeverInstantiated.Global
public class ValidateModelFilter : ActionFilterAttribute
{
    public override Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!context.ModelState.IsValid)
            context.Result = new BadRequestObjectResult(context.ModelState);
        return base.OnActionExecutionAsync(context, next);
    }
}