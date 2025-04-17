using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Fluffle.Vector.Api.Client;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddVectorApiClient(this IServiceCollection services)
    {
        services.AddOptions<VectorApiClientOptions>()
            .BindConfiguration(VectorApiClientOptions.VectorApiClient)
            .ValidateDataAnnotations().ValidateOnStart();

        services.AddHttpClient(nameof(VectorApiClient), (serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<VectorApiClientOptions>>();
            client.BaseAddress = new Uri(options.Value.Url);

            client.DefaultRequestHeaders.Add("Api-Key", options.Value.ApiKey);
        });
        services.AddSingleton<IVectorApiClient, VectorApiClient>();

        return services;
    }
}
