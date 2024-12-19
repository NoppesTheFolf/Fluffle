using Microsoft.Extensions.DependencyInjection;
using Noppes.Fluffle.Configuration;
using Noppes.Fluffle.Database;
using Noppes.Fluffle.Search.Business.Repositories;
using Noppes.Fluffle.Search.Database.Repositories;

namespace Noppes.Fluffle.Search.Database;

public static class ServiceCollectionExtensions
{
    public static void AddEntityFramework(this IServiceCollection services, FluffleConfiguration fluffleConfiguration)
    {
        services.AddDatabase<FluffleSearchContext, SearchDatabaseConfiguration>(fluffleConfiguration);

        services.AddTransient<IPlatformRepository, PlatformRepository>();
        services.AddTransient<IImageRepository, ImageRepository>();
    }
}
