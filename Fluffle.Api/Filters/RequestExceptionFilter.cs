using Flurl.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Noppes.Fluffle.Database;
using Noppes.Fluffle.Http;
using Npgsql;
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

            TracedV1Error error = new()
            {
                TraceId = requestId
            };

            if (context.Exception is NpgsqlException npgsqlException && npgsqlException.IsTransient() ||
                context.Exception is FlurlHttpException httpException && httpException.IsTransient() ||
                context.Exception is FlurlHttpTimeoutException)
            {
                error.Code = "UNAVAILABLE";
                error.Message = "Fluffle is partially offline.";
                Handle(context, error, 503); // 503 Service Unavailable
                return;
            }

            error.Code = "KABOOM";
            error.Message = "A non-transient error occurred at Fluffle's side. " +
                            "If you can reproduce this issue, then please consider contacting us so that we can resolve the issue (see https://fluffle.xyz/contact).";

            Handle(context, error, 500); // 500 Internal server error
        }

        private static void Handle(ActionExecutedContext context, V1Error error, int statusCode)
        {
            context.Result = new ObjectResult(error)
            {
                StatusCode = statusCode
            };
            context.ExceptionHandled = true;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
        }
    }
}
