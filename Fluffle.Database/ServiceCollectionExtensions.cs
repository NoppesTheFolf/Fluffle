using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Noppes.Fluffle.Configuration;
using System;

namespace Noppes.Fluffle.Database
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDatabase<TContext, TConfiguration>(this IServiceCollection services, FluffleConfiguration configuration)
            where TContext : BaseContext where TConfiguration : DatabaseConfiguration
        {
            services.AddDbContext<TContext>(options =>
            {
                var dbConf = configuration.Get<TConfiguration>();
                options.UseNpgsql(dbConf.ConnectionString);

                if (!dbConf.EnableLogging)
                    return;

                options.LogTo(Console.WriteLine);
                options.EnableSensitiveDataLogging();
            });

            return services;
        }
    }
}
