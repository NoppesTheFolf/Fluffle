using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;

namespace Fluffle.Ingestion.Worker.ApplicationInsights;

public class CloudRoleNameInitializer : ITelemetryInitializer
{
    public void Initialize(ITelemetry telemetry)
    {
        telemetry.Context.Cloud.RoleName = "IngestionWorker";
    }
}
