using Fluffle.Feeder.Framework.StatePersistence;
using Fluffle.Feeder.Framework.StatePersistence.Cosmos;
using Fluffle.Ingestion.Api.Client;
using Microsoft.Extensions.DependencyInjection;

namespace Fluffle.Feeder.Framework;

public static class ServiceCollectionExtensions
{
    public static void AddFeederTemplate(this IServiceCollection services)
    {
        // State persistence
        services.AddOptions<CosmosOptions>()
            .BindConfiguration(CosmosOptions.Cosmos)
            .ValidateDataAnnotations().ValidateOnStart();

        services.AddSingleton<CosmosClientFactory>();
        services.AddSingleton<IStateRepositoryFactory, CosmosStateRepositoryFactory>();

        // Ingestion
        services.AddIngestionApiClient();
    }
}
