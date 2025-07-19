using Fluffle.Content.Api.Client;
using Fluffle.Imaging.Api.Client;
using Fluffle.Inference.Api.Client;
using Fluffle.Ingestion.Api.Client;
using Fluffle.Ingestion.Worker;
using Fluffle.Ingestion.Worker.ApplicationInsights;
using Fluffle.Ingestion.Worker.ItemActionHandlers;
using Fluffle.Ingestion.Worker.ItemContentClient;
using Fluffle.Ingestion.Worker.ThumbnailStorage;
using Fluffle.Vector.Api.Client;
using Microsoft.ApplicationInsights.Extensibility;

var builder = Host.CreateApplicationBuilder(args);
var services = builder.Services;

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

services.AddIngestionApiClient();

services.AddImagingApiClient();

services.AddInferenceApiClient();

services.AddContentApiClient();

services.AddVectorApiClient();

services.AddHttpClient(nameof(ItemContentClient), client =>
{
    client.DefaultRequestHeaders.Add("User-Agent", "fluffle.xyz by NoppesTheFolf");
}).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    AllowAutoRedirect = false
});

services.AddSingleton<IItemContentClient, ItemContentClient>();

services.AddOptions<ThumbnailStorageOptions>()
    .BindConfiguration(ThumbnailStorageOptions.ThumbnailStorage)
    .ValidateDataAnnotations().ValidateOnStart();
services.AddSingleton<IThumbnailStorage, ContentApiThumbnailStorage>();

services.AddSingleton<ItemActionHandlerFactory>();

services.AddOptions<WorkerOptions>()
    .BindConfiguration(WorkerOptions.Worker)
    .ValidateDataAnnotations().ValidateOnStart();

var workerCount = builder.Configuration.GetValue<int>("Worker:Count");
for (var i = 0; i < workerCount; i++)
    services.AddSingleton<IHostedService, Worker>();

var host = builder.Build();
await host.RunAndSetExitCodeAsync();
