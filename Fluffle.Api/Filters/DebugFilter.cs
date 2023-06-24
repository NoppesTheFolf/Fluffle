using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Noppes.Fluffle.Api.Controllers;
using Noppes.Fluffle.Utils;

namespace Noppes.Fluffle.Api.Filters;

public class DebugFilter : IActionFilter
{
    public static string DebugKey { get; } = RandomString.Generate(32);

    private readonly ILogger<DebugFilter> _logger;

    public DebugFilter(ILogger<DebugFilter> logger)
    {
        _logger = logger;
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.HttpContext.Request.Headers.TryGetValue("debug-key", out var debugKey))
            return;

        if (debugKey != DebugKey)
        {
            _logger.LogWarning("Invalid debug key provided.");
            return;
        }

        var controller = (ApiController)context.Controller;
        controller.IsDebug = true;
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
    }
}
