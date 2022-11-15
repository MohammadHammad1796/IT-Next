using IT_Next.Core.Extensions;
using Microsoft.AspNetCore.Mvc.Filters;

namespace IT_Next.Custom.FilterAttributes;

// ReSharper disable once ClassNeverInstantiated.Global
public class TrimStringsFilter : ActionFilterAttribute
{
    public override Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var inputs = context.ActionArguments.Values;
        foreach (var model in inputs)
        {
            if (model is null)
                continue;

            var properties = model.GetType().GetProperties();
            foreach (var property in properties)
            {
                if (property.PropertyType != typeof(string))
                    continue;

                var stringValue = property.GetValue(model)?.ToString();

                property.SetValue(model, stringValue.TrimExtraSpaces());
            }
        }

        return base.OnActionExecutionAsync(context, next);
    }
}