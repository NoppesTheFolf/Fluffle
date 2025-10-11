using Fluffle.Feeder.Bluesky.JetstreamWatcher;
using Fluffle.Feeder.Bluesky.Mongo;
using Fluffle.Feeder.Framework;

var builder = Host.CreateApplicationBuilder(args);
var services = builder.Services;

services.AddFeederApplicationInsights("BlueskyJetstreamWatcher");

services.AddFeederStatePersistence();

services.AddMongo();

services.AddOptions<BlueskyJetstreamWatcherOptions>()
    .BindConfiguration(BlueskyJetstreamWatcherOptions.BlueskyJetstreamWatcher)
    .ValidateDataAnnotations().ValidateOnStart();

services.AddHostedService<Worker>();

var host = builder.Build();
await host.RunAndSetExitCodeAsync();
