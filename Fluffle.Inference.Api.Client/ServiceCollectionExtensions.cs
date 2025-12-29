using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Fluffle.Inference.Api.Client;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInferenceApiClient(this IServiceCollection services)
    {
        services.AddOptions<InferenceApiClientOptions>()
            .BindConfiguration(InferenceApiClientOptions.InferenceApiClient)
            .ValidateDataAnnotations().ValidateOnStart();

        services.AddHttpClient(nameof(InferenceApiClient), (serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<InferenceApiClientOptions>>();
            client.BaseAddress = new Uri(options.Value.Url);

            client.DefaultRequestHeaders.Add("Api-Key", options.Value.ApiKey);
        });
        services.AddSingleton<IInferenceApiClient, InferenceApiClient>();

        return services;
    }
}
