using Fluffle.Feeder.E621;
using Fluffle.Feeder.Framework;
using Microsoft.Extensions.Options;
using Noppes.E621;

var builder = Host.CreateApplicationBuilder(args);
var services = builder.Services;

services.AddFeederTemplate();

builder.Services.AddOptions<E621FeederOptions>()
    .BindConfiguration(E621FeederOptions.E621Feeder)
    .ValidateDataAnnotations().ValidateOnStart();

builder.Services.AddOptions<E621ApiClientOptions>()
    .BindConfiguration(E621ApiClientOptions.E621ApiClient)
    .ValidateDataAnnotations().ValidateOnStart();

builder.Services.AddSingleton(provider =>
{
    var options = provider.GetRequiredService<IOptions<E621ApiClientOptions>>();

    var e621Client = new E621ClientBuilder()
        .WithUserAgent("Fluffle", "main", "NoppesTheFolf", "Everywhere")
        .Build();

    e621Client.LogInAsync(options.Value.Username, options.Value.ApiKey, skipValidation: true).Wait();

    return e621Client;
});

services.AddHostedService<RecentWorker>();
services.AddHostedService<CompleteWorker>();

var host = builder.Build();
await host.RunAndSetExitCodeAsync();
