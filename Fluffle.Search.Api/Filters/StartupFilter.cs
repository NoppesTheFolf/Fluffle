using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Noppes.Fluffle.Api;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;

namespace Noppes.Fluffle.Search.Api.Filters;

public class StartupFilter : IActionFilter
{
    public static readonly V1Error StartingUpError = new("UNAVAILABLE", "The server isn't ready to handle requests yet.");

    public static volatile bool HasStarted = false;

    public void OnActionExecuting(ActionExecutingContext context)
    {
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        if (HasStarted || context.Exception == null || context.ExceptionHandled)
            return;

        if (context.Exception.InnerException is HttpRequestException { InnerException: SocketException })
        {
            context.Result = new ObjectResult(StartingUpError)
            {
                StatusCode = (int)HttpStatusCode.ServiceUnavailable
            };
            context.ExceptionHandled = true;
        }
    }
}
