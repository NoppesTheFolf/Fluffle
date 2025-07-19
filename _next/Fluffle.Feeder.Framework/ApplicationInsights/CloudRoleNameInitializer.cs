using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Options;

namespace Fluffle.Feeder.Framework.ApplicationInsights;

internal class CloudRoleNameInitializer : ITelemetryInitializer
{
    private readonly IOptions<ApplicationInsightsOptions> _options;

    public CloudRoleNameInitializer(IOptions<ApplicationInsightsOptions> options)
    {
        _options = options;
    }

    public void Initialize(ITelemetry telemetry)
    {
        telemetry.Context.Cloud.RoleName = _options.Value.CloudRoleName;
    }
}
