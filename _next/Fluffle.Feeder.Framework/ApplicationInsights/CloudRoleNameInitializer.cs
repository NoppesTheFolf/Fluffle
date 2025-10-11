using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;

namespace Fluffle.Feeder.Framework.ApplicationInsights;

internal class CloudRoleNameInitializer : ITelemetryInitializer
{
    private readonly string _cloudRoleName;

    public CloudRoleNameInitializer(string cloudRoleName)
    {
        _cloudRoleName = cloudRoleName;
    }

    public void Initialize(ITelemetry telemetry)
    {
        telemetry.Context.Cloud.RoleName = _cloudRoleName;
    }
}
