using Fluffle.Feeder.Framework;
using Fluffle.Feeder.Legacy;
using Fluffle.Feeder.Legacy.MainApi;
using Microsoft.Extensions.Options;

var builder = Host.CreateApplicationBuilder(args);
var services = builder.Services;

services.AddFeederTemplate();

services.AddOptions<LegacyFeederOptions>()
    .BindConfiguration(LegacyFeederOptions.LegacyFeeder)
    .ValidateDataAnnotations().ValidateOnStart();

services.AddOptions<MainApiClientOptions>()
    .BindConfiguration(MainApiClientOptions.MainApiClient)
    .ValidateDataAnnotations().ValidateOnStart();

services.AddHttpClient(nameof(MainApiClient), (serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<IOptions<MainApiClientOptions>>();
    client.BaseAddress = new Uri(options.Value.Url);

    client.DefaultRequestHeaders.Add("Api-Key", options.Value.ApiKey);
});
services.AddSingleton<MainApiClient>();

services.AddHostedService<Worker>();

var host = builder.Build();
await host.RunAndSetExitCodeAsync();
