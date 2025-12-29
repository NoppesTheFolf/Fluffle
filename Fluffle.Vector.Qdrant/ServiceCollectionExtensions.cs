using Fluffle.Vector.Core.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Qdrant.Client;

namespace Fluffle.Vector.Qdrant;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddQdrant(this IServiceCollection services)
    {
        services.AddOptions<QdrantOptions>()
            .BindConfiguration(QdrantOptions.Qdrant)
            .ValidateDataAnnotations().ValidateOnStart();

        services.AddSingleton(provider =>
        {
            var options = provider.GetRequiredService<IOptions<QdrantOptions>>();
            return new QdrantClient(
                host: options.Value.Host,
                apiKey: options.Value.ApiKey
            );
        });

        services.AddSingleton<IItemVectorsRepository, QdrantItemVectorsRepository>();

        return services;
    }
}
