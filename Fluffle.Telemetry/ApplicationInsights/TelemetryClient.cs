using Microsoft.ApplicationInsights.DataContracts;
using AiTelemetryClient = Microsoft.ApplicationInsights.TelemetryClient;

namespace Noppes.Fluffle.Telemetry.ApplicationInsights;

internal class TelemetryClient : ITelemetryClient
{
    private readonly AiTelemetryClient _aiTelemetryClient;
    private readonly string _trackedBy;

    public TelemetryClient(AiTelemetryClient aiTelemetryClient, string trackedBy)
    {
        _aiTelemetryClient = aiTelemetryClient;
        _trackedBy = trackedBy;
    }

    public Task TrackExceptionAsync(Exception exception, string? operationId = null)
    {
        var exceptionTelemetry = new ExceptionTelemetry
        {
            Exception = exception,
            Properties =
            {
                ["trackedBy"] = _trackedBy
            }
        };
        if (!string.IsNullOrEmpty(operationId))
            exceptionTelemetry.Context.Operation.Id = operationId;

        _aiTelemetryClient.TrackException(exceptionTelemetry);

        return Task.CompletedTask;
    }

    public async Task FlushAsync() => await _aiTelemetryClient.FlushAsync(CancellationToken.None);
}
