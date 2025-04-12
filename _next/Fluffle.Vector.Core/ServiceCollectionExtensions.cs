using Fluffle.Vector.Core.Services;
using Fluffle.Vector.Core.Vectors;
using Microsoft.Extensions.DependencyInjection;

namespace Fluffle.Vector.Core;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCore(this IServiceCollection services)
    {
        services.AddSingleton<ItemService>();
        services.AddSingleton<VectorCollection>();
        services.AddHostedService<VectorCollectionInitializer>();

        return services;
    }
}
