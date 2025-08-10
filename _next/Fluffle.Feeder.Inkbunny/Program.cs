using Fluffle.Feeder.Framework;
using Fluffle.Feeder.Framework.HttpClient;
using Fluffle.Feeder.Inkbunny;
using Fluffle.Feeder.Inkbunny.Client;
using Microsoft.Extensions.Options;
using System.Net;

var builder = Host.CreateApplicationBuilder(args);
var services = builder.Services;

services.AddFeederTemplate();

services.AddOptions<InkbunnyFeederOptions>()
    .BindConfiguration(InkbunnyFeederOptions.InkbunnyFeeder)
    .ValidateDataAnnotations().ValidateOnStart();

services.AddOptions<InkbunnyClientOptions>()
    .BindConfiguration(InkbunnyClientOptions.InkbunnyClient)
    .ValidateDataAnnotations().ValidateOnStart();

services.AddHttpClient(nameof(InkbunnyClient), client =>
{
    client.BaseAddress = new Uri("https://inkbunny.net");
    client.DefaultRequestHeaders.Add("User-Agent", "fluffle.xyz by NoppesTheFolf");
}).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    AutomaticDecompression = DecompressionMethods.All
}).AddPacedRateLimit(provider => provider.GetRequiredService<IOptions<InkbunnyClientOptions>>().Value.RateLimitPace);
services.AddSingleton<InkbunnyClient>();

services.AddHostedService<RecentWorker>();
services.AddHostedService<ArchiveWorker>();

var host = builder.Build();
await host.RunAndSetExitCodeAsync();
