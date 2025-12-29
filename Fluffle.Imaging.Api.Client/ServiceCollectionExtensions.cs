using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Fluffle.Imaging.Api.Client;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddImagingApiClient(this IServiceCollection services)
    {
        services.AddOptions<ImagingApiClientOptions>()
            .BindConfiguration(ImagingApiClientOptions.ImagingApiClient)
            .ValidateDataAnnotations().ValidateOnStart();

        services.AddHttpClient(nameof(ImagingApiClient), (serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<ImagingApiClientOptions>>();
            client.BaseAddress = new Uri(options.Value.Url);

            client.DefaultRequestHeaders.Add("Api-Key", options.Value.ApiKey);
        });
        services.AddSingleton<IImagingApiClient, ImagingApiClient>();

        return services;
    }
}
