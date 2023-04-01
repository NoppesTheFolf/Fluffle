namespace Noppes.Fluffle.Telemetry;

public interface ITelemetryClientFactory
{
    ITelemetryClient Create(string trackedBy);
}
