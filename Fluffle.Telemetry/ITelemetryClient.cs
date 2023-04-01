namespace Noppes.Fluffle.Telemetry;

public interface ITelemetryClient
{
    Task TrackExceptionAsync(Exception exception, string? operationId = null);

    Task FlushAsync();
}
