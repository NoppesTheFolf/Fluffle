using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Fluffle.Content.Api.Client;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddContentApiClient(this IServiceCollection services)
    {
        services.AddOptions<ContentApiClientOptions>()
            .BindConfiguration(ContentApiClientOptions.ContentApiClient)
            .ValidateDataAnnotations().ValidateOnStart();

        services.AddHttpClient(nameof(ContentApiClient), (serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<ContentApiClientOptions>>();
            client.BaseAddress = new Uri(options.Value.Url);

            client.DefaultRequestHeaders.Add("Api-Key", options.Value.ApiKey);
        });
        services.AddSingleton<IContentApiClient, ContentApiClient>();

        return services;
    }
}
