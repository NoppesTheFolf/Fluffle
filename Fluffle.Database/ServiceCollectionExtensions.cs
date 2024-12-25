using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Noppes.Fluffle.Configuration;

namespace Noppes.Fluffle.Database;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDatabase<TContext, TConfiguration>(this IServiceCollection services, FluffleConfiguration configuration)
        where TContext : BaseContext where TConfiguration : DatabaseConfiguration
    {
        services.AddDbContext<TContext>(options =>
        {
            var dbConf = configuration.Get<TConfiguration>();
            options.UseNpgsql(dbConf.ConnectionString, builder =>
            {
                builder.CommandTimeout(dbConf.CommandTimeout);
            });
        });

        return services;
    }
}
