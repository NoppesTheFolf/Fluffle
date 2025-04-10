using Fluffle.Ingestion.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Fluffle.Ingestion.Core;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCore(this IServiceCollection services)
    {
        services.AddSingleton<ItemActionService>();

        return services;
    }
}
