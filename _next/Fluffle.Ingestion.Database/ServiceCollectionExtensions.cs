using Fluffle.Ingestion.Core.Repositories;
using Fluffle.Ingestion.Database.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Fluffle.Ingestion.Database;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMongo(this IServiceCollection services)
    {
        services.AddOptions<MongoOptions>()
            .BindConfiguration(MongoOptions.Mongo)
            .ValidateDataAnnotations().ValidateOnStart();

        services.AddSingleton<MongoContext>();
        services.AddSingleton<IItemActionRepository, MongoItemActionRepository>();

        return services;
    }
}
