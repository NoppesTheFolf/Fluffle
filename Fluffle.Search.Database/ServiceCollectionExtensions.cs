using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Noppes.Fluffle.Configuration;
using Noppes.Fluffle.Search.Business.Repositories;
using Noppes.Fluffle.Search.Database.Repositories;

namespace Noppes.Fluffle.Search.Database;

public static class ServiceCollectionExtensions
{
    public static void AddEntityFramework(this IServiceCollection services, FluffleConfiguration fluffleConfiguration)
    {
        var conf = fluffleConfiguration.Get<SearchDatabaseConfiguration>();
        services.AddDbContext<FluffleSearchContext>(dbContextOptions =>
        {
            dbContextOptions.UseNpgsql(conf.ConnectionString, npgsqlOptions =>
            {
                npgsqlOptions.CommandTimeout(conf.CommandTimeout);
            });
        });

        services.AddTransient<IPlatformRepository, PlatformRepository>();
        services.AddTransient<IImageRepository, ImageRepository>();
    }
}
