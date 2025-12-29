using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Fluffle.Ingestion.Api.Client;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddIngestionApiClient(this IServiceCollection services)
    {
        services.AddOptions<IngestionApiClientOptions>()
            .BindConfiguration(IngestionApiClientOptions.IngestionApiClient)
            .ValidateDataAnnotations().ValidateOnStart();

        services.AddHttpClient(nameof(IngestionApiClient), (serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<IngestionApiClientOptions>>();
            client.BaseAddress = new Uri(options.Value.Url);

            client.DefaultRequestHeaders.Add("Api-Key", options.Value.ApiKey);
        });
        services.AddSingleton<IIngestionApiClient, IngestionApiClient>();

        return services;
    }
}
