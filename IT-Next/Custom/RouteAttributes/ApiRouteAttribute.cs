using Microsoft.AspNetCore.Mvc;

namespace IT_Next.Custom.RouteAttributes;

public class ApiRouteAttribute : RouteAttribute
{
    public ApiRouteAttribute(string template)
        : base($"api/{template}")
    {
    }
}