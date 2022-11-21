using IT_Next.Custom.RouteAttributes;
using System.Text.Json;

namespace IT_Next.Custom.Middlewares;

public class DisableHttpOnApiMiddleware
{
    private readonly RequestDelegate _next;

    public DisableHttpOnApiMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.IsHttps)
        { await _next(context); return; }

        var requestPath = context.Request.Path.ToString();
        var apiPath = string.Concat("/", ApiRouteAttribute.MainRoute);
        if (!requestPath.StartsWith(apiPath, StringComparison.CurrentCultureIgnoreCase))
        { await _next(context); return; }

        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        context.Response.ContentType = "application/json";
        var model = new { Error = "Request should be over https." };
        var jsonString = JsonSerializer.Serialize(model);
        await context.Response.WriteAsync(jsonString);
    }
}