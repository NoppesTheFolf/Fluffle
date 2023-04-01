using AiTelemetryClient = Microsoft.ApplicationInsights.TelemetryClient;

namespace Noppes.Fluffle.Telemetry.ApplicationInsights;

internal class TelemetryClientFactory : ITelemetryClientFactory
{
    private readonly AiTelemetryClient _aiTelemetryClient;

    public TelemetryClientFactory(AiTelemetryClient aiTelemetryClient)
    {
        _aiTelemetryClient = aiTelemetryClient;
    }

    public ITelemetryClient Create(string trackedBy) => new TelemetryClient(_aiTelemetryClient, trackedBy);
}
