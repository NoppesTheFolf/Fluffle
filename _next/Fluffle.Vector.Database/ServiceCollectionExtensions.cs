using Fluffle.Vector.Core.Repositories;
using Fluffle.Vector.Database.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Fluffle.Vector.Database;

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
