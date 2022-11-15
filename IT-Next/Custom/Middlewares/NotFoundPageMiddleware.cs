namespace IT_Next.Custom.Middlewares;

public class NotFoundPageMiddleware
{
    private readonly RequestDelegate _next;

    public NotFoundPageMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        await _next(context);

        const int notFoundStatusCode = 404;
        var responseStatusCode = context.Response.StatusCode;
        if (responseStatusCode != notFoundStatusCode)
            return;

        var contentType = context.Response.ContentType;
        if (!string.IsNullOrEmpty(contentType))
            return;

        // controllerRoute/actionRoute
        context.Request.Path = "/notFound";
        await _next(context);
    }
}