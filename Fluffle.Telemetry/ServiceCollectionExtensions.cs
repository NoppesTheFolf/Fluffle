using Microsoft.Extensions.DependencyInjection;
using Noppes.Fluffle.Configuration;
using Noppes.Fluffle.Telemetry.ApplicationInsights;
using AiTelemetryClient = Microsoft.ApplicationInsights.TelemetryClient;
using AiTelemetryConfiguration = Microsoft.ApplicationInsights.Extensibility.TelemetryConfiguration;

namespace Noppes.Fluffle.Telemetry;

public static class ServiceCollectionExtensions
{
    public static void AddTelemetry(this IServiceCollection services, FluffleConfiguration configuration, string appName)
    {
        var aiConfiguration = configuration.Get<ApplicationInsightsConfiguration>();

        var aiTelemetryConfiguration = AiTelemetryConfiguration.CreateDefault();
        aiTelemetryConfiguration.ConnectionString = aiConfiguration.ConnectionString;
        var aiTelemetryClient = new AiTelemetryClient(aiTelemetryConfiguration);
        aiTelemetryClient.Context.Cloud.RoleName = appName;

        var telemetryClientFactory = new TelemetryClientFactory(aiTelemetryClient);
        services.AddSingleton<ITelemetryClientFactory>(telemetryClientFactory);
    }
}
