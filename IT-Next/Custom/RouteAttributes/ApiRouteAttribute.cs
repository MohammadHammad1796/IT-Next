using Microsoft.AspNetCore.Mvc;

namespace IT_Next.Custom.RouteAttributes;

public class ApiRouteAttribute : RouteAttribute
{
    public const string MainRoute = "api/";
    
    public ApiRouteAttribute(string template)
        : base($"{MainRoute}{template}")
    {
    }
}