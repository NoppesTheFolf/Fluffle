using Fluffle.Vector.Core.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Fluffle.Vector.Core;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCore(this IServiceCollection services)
    {
        services.AddSingleton<IModelRepository, PredefinedModelRepository>();

        return services;
    }
}
