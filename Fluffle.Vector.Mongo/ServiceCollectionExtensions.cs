using Fluffle.Vector.Core.Repositories;
using Fluffle.Vector.Mongo.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Fluffle.Vector.Mongo;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMongo(this IServiceCollection services)
    {
        services.AddOptions<MongoOptions>()
            .BindConfiguration(MongoOptions.Mongo)
            .ValidateDataAnnotations().ValidateOnStart();

        services.AddSingleton<MongoContext>();
        services.AddSingleton<IItemRepository, MongoItemRepository>();

        return services;
    }
}
