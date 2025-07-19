using Fluffle.Feeder.Framework.ApplicationInsights;
using Fluffle.Feeder.Framework.StatePersistence;
using Fluffle.Feeder.Framework.StatePersistence.Cosmos;
using Fluffle.Ingestion.Api.Client;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.DependencyInjection;

namespace Fluffle.Feeder.Framework;

public static class ServiceCollectionExtensions
{
    public static void AddFeederTemplate(this IServiceCollection services)
    {
        // Application Insights
        services.AddOptions<ApplicationInsightsOptions>()
            .BindConfiguration(ApplicationInsightsOptions.ApplicationInsights)
            .ValidateDataAnnotations().ValidateOnStart();

        services.AddSingleton<ITelemetryInitializer, CloudRoleNameInitializer>();
        services.AddHostedService<ApplicationInsightsFlushService>();
        services.AddApplicationInsightsTelemetryWorkerService(options =>
        {
            options.EnableQuickPulseMetricStream = true; // No telemetry when this is disabled... ???
            options.EnableAdaptiveSampling = true;

            options.EnablePerformanceCounterCollectionModule = false;
            options.EnableDependencyTrackingTelemetryModule = false;
            options.EnableEventCounterCollectionModule = false;
            options.AddAutoCollectedMetricExtractor = false;
            options.EnableDiagnosticsTelemetryModule = false;
        });

        // State persistence
        services.AddOptions<CosmosOptions>()
            .BindConfiguration(CosmosOptions.Cosmos)
            .ValidateDataAnnotations().ValidateOnStart();

        services.AddSingleton<CosmosClientFactory>();
        services.AddSingleton<IStateRepositoryFactory, CosmosStateRepositoryFactory>();

        // Ingestion
        services.AddIngestionApiClient();
    }
}
