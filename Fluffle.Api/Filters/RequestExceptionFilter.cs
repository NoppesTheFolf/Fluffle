using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;

namespace Noppes.Fluffle.Api.Filters
{
    /// <summary>
    /// Handles exceptions thrown by controller actions. Logs the exception to the console,
    /// including the trace ID, and returns an error response in the <see cref="TracedV1Error"/> format.
    /// </summary>
    public class RequestExceptionFilter : IActionFilter
    {
        private readonly ILogger<RequestExceptionFilter> _logger;

        public RequestExceptionFilter(ILogger<RequestExceptionFilter> logger)
        {
            _logger = logger;
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // No exception occurred
            if (context.Exception == null)
                return;

            if (!(context.Controller is ControllerBase fluffleController))
                throw new InvalidOperationException($"The {nameof(RequestExceptionFilter)} can only be used on instances of {nameof(ControllerBase)}.");

            var requestId = fluffleController.HttpContext.TraceIdentifier;

            _logger.LogError(context.Exception, "Exception caught for request with ID {requestId}.", requestId);

            var error = new TracedV1Error("UNKNOWN", context.HttpContext.TraceIdentifier,
                "Welp, something went horribly wrong at Fluffle's side. " +
                "If you can reproduce this issue and think it's a bug, then please contact us.");

            context.Result = new ObjectResult(error)
            {
                StatusCode = 500 // 500: Internal server error
            };
            context.ExceptionHandled = true;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
        }
    }
}
