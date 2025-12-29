using Fluffle.Feeder.Bluesky.Core.Domain.Events;
using Fluffle.Feeder.Bluesky.JetstreamProcessor;
using Fluffle.Feeder.Bluesky.JetstreamProcessor.ApiClient;
using Fluffle.Feeder.Bluesky.JetstreamProcessor.EventHandlers;
using Fluffle.Feeder.Bluesky.Mongo;
using Fluffle.Feeder.Framework;
using Fluffle.Inference.Api.Client;
using Fluffle.Ingestion.Api.Client;
using System.Threading.Channels;

var builder = Host.CreateApplicationBuilder(args);
var services = builder.Services;

services.AddOptions<BlueskyJetstreamProcessorOptions>()
    .BindConfiguration(BlueskyJetstreamProcessorOptions.BlueskyJetstreamProcessor)
    .ValidateDataAnnotations().ValidateOnStart();

services.AddFeederApplicationInsights("BlueskyJetstreamProcessor");

services.AddInferenceApiClient();

services.AddIngestionApiClient();

services.AddMongo();

services.AddHttpClient(nameof(BlueskyApiClient), client =>
{
    client.DefaultRequestHeaders.Add("User-Agent", "fluffle.xyz by NoppesTheFolf");
}).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    AllowAutoRedirect = false,
    UseCookies = false
});
services.AddSingleton<IBlueskyApiClient, BlueskyApiClient>();

services.AddSingleton<BlueskyEventHandlerFactory>();

services.AddSingleton(Channel.CreateBounded<BlueskyEvent>(2));
services.AddHostedService<DequeueWorker>();
var workerCount = builder.Configuration.GetValue<int>("BlueskyJetstreamProcessor:WorkerCount");
for (var i = 0; i < workerCount; i++)
{
    services.AddSingleton<IHostedService, ProcessWorker>();
}

var host = builder.Build();
await host.RunAndSetExitCodeAsync();
