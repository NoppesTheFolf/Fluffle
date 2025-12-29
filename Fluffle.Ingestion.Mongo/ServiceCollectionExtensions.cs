using Fluffle.Ingestion.Core.Repositories;
using Fluffle.Ingestion.Mongo.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Fluffle.Ingestion.Mongo;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMongo(this IServiceCollection services)
    {
        services.AddOptions<MongoOptions>()
            .BindConfiguration(MongoOptions.Mongo)
            .ValidateDataAnnotations().ValidateOnStart();

        services.AddSingleton<MongoContext>();
        services.AddSingleton<IItemActionRepository, MongoItemActionRepository>();
        services.AddSingleton<IItemActionFailureRepository, MongoItemActionFailureRepository>();

        return services;
    }
}
