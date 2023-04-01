using Flurl.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Noppes.Fluffle.Database;
using Noppes.Fluffle.Http;
using Noppes.Fluffle.Telemetry;
using Npgsql;
using System.Net;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Api.Filters
{
    /// <summary>
    /// Handles exceptions thrown by controller actions. Logs the exception to the console,
    /// including the trace ID, and returns an error response in the <see cref="TracedV1Error"/> format.
    /// </summary>
    public class RequestExceptionFilter : IAsyncExceptionFilter
    {
        private readonly ITelemetryClient _telemetryClient;
        private readonly ILogger<RequestExceptionFilter> _logger;

        public RequestExceptionFilter(ITelemetryClientFactory telemetryClientFactory, ILogger<RequestExceptionFilter> logger)
        {
            _telemetryClient = telemetryClientFactory.Create(nameof(RequestExceptionFilter));
            _logger = logger;
        }

        public async Task OnExceptionAsync(ExceptionContext context)
        {
            if (context.ExceptionHandled)
                return;

            var requestId = context.HttpContext.TraceIdentifier;
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
                Handle(context, error, HttpStatusCode.ServiceUnavailable);
                return;
            }

            error.Code = "KABOOM";
            error.Message = "A non-transient error occurred at Fluffle's side. " +
                            "If you can reproduce this issue, then please consider contacting us so that we can resolve the issue (see https://fluffle.xyz/contact).";

            await _telemetryClient.TrackExceptionAsync(context.Exception, requestId);

            Handle(context, error, HttpStatusCode.InternalServerError);
        }

        private static void Handle(ExceptionContext context, TracedV1Error error, HttpStatusCode statusCode)
        {
            context.Result = new ObjectResult(error)
            {
                StatusCode = (int)statusCode
            };
            context.ExceptionHandled = true;
        }
    }
}
