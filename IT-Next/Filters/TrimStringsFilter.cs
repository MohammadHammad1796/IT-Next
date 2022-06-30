using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Filters;

namespace IT_Next.Filters;

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
                if (string.IsNullOrWhiteSpace(stringValue))
                    continue;

                stringValue = stringValue.Trim();
                stringValue = Regex.Replace(stringValue, @"\s+", " ");
                property.SetValue(model, stringValue);
            }
        }

        return base.OnActionExecutionAsync(context, next);
    }
}