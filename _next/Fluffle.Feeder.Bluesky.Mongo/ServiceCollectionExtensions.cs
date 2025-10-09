using Fluffle.Feeder.Bluesky.Core.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Fluffle.Feeder.Bluesky.Mongo;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMongo(this IServiceCollection services)
    {
        services.AddOptions<MongoOptions>()
            .BindConfiguration(MongoOptions.Mongo)
            .ValidateDataAnnotations().ValidateOnStart();

        services.AddSingleton<MongoContext>();
        services.AddSingleton<IBlueskyEventRepository, MongoBlueskyEventRepository>();
        services.AddSingleton<IBlueskyProfileRepository, MongoBlueskyProfileRepository>();
        services.AddSingleton<IBlueskyPostRepository, MongoBlueskyPostRepository>();

        return services;
    }
}
