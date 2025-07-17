using Fluffle.Feeder.Framework;
using Fluffle.Feeder.Framework.HttpClient;
using Fluffle.Feeder.FurAffinity;
using Fluffle.Feeder.FurAffinity.Client;
using Fluffle.Feeder.FurAffinity.Workers;
using Microsoft.Extensions.Options;

var builder = Host.CreateApplicationBuilder(args);
var services = builder.Services;

services.AddFeederTemplate();

services.AddOptions<FurAffinityFeederOptions>()
    .BindConfiguration(FurAffinityFeederOptions.FurAffinityFeeder)
    .ValidateDataAnnotations().ValidateOnStart();

services.AddOptions<FurAffinityClientOptions>()
    .BindConfiguration(FurAffinityClientOptions.FurAffinityClient)
    .ValidateDataAnnotations().ValidateOnStart();

services.AddHttpClient(nameof(FurAffinityClient), (serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<IOptions<FurAffinityClientOptions>>();
    client.BaseAddress = new Uri("https://www.furaffinity.net");

    client.DefaultRequestHeaders.Add("User-Agent", "fluffle.xyz by NoppesTheFolf");
    client.DefaultRequestHeaders.Add("Cookie", $"a={options.Value.A}; b={options.Value.B}");
}).AddPacedRateLimit(provider => provider.GetRequiredService<IOptions<FurAffinityClientOptions>>().Value.RateLimitPace);

services.AddSingleton<FurAffinityClient>();

services.AddHostedService<AgedWorker>();
services.AddHostedService<NewestWorker>();
services.AddHostedService<ArchiveWorker>();

var host = builder.Build();
host.Run();
