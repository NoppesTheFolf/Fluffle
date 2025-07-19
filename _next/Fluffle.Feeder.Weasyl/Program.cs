using Fluffle.Feeder.Framework;
using Fluffle.Feeder.Framework.HttpClient;
using Fluffle.Feeder.Weasyl;
using Fluffle.Feeder.Weasyl.ApiClient;
using Fluffle.Feeder.Weasyl.Workers;
using Microsoft.Extensions.Options;

var builder = Host.CreateApplicationBuilder(args);
var services = builder.Services;

services.AddFeederTemplate();

services.AddOptions<WeasylFeederOptions>()
    .BindConfiguration(WeasylFeederOptions.WeasylFeeder)
    .ValidateDataAnnotations().ValidateOnStart();

services.AddOptions<WeasylApiClientOptions>()
    .BindConfiguration(WeasylApiClientOptions.WeasylApiClient)
    .ValidateDataAnnotations().ValidateOnStart();

services.AddHttpClient(nameof(WeasylApiClient), (serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<IOptions<WeasylApiClientOptions>>();
    client.BaseAddress = new Uri("https://www.weasyl.com");

    client.DefaultRequestHeaders.Add("User-Agent", "fluffle.xyz by NoppesTheFolf");
    client.DefaultRequestHeaders.Add("X-Weasyl-API-Key", options.Value.ApiKey);
}).AddPacedRateLimit(provider => provider.GetRequiredService<IOptions<WeasylApiClientOptions>>().Value.RateLimitPace);
services.AddSingleton<WeasylApiClient>();

services.AddHostedService<NewestWorker>();
services.AddHostedService<ArchiveWorker>();

var host = builder.Build();
await host.RunAndSetExitCodeAsync();
