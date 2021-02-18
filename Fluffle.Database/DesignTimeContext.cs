using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;
using Noppes.Fluffle.Configuration;
using System;

namespace Noppes.Fluffle.Database
{
    /// <summary>
    /// Entity Framework Core its commandline tool (which is used for managing migrations) needs
    /// some way of knowing we're using PostgreSQL (and the Npgsql provider). Sadly, concrete
    /// classes need to be created for each implementation of <see cref="BaseContext"/>. So, each
    /// concrete implementation of <see cref="BaseContext"/> also needs a concrete implementation of
    /// this class.
    /// </summary>
    public abstract class DesignTimeContext<TContext> : IDesignTimeDbContextFactory<TContext> where TContext : BaseContext
    {
        public TContext CreateDbContext(string[] args)
        {
            IServiceCollection services = new ServiceCollection();

            var context = Activator.CreateInstance<TContext>();
            services.AddDbContext<TContext>(options =>
            {
                var configuration = (DatabaseConfiguration)FluffleConfiguration.Load(typeof(TContext)).Get(context.ConfigurationType);

                options.UseNpgsql(configuration.ConnectionString, npgsqlOptions =>
                {
                    npgsqlOptions.CommandTimeout((int)Math.Floor(10.Minutes().TotalSeconds));
                });
            });

            return services.BuildServiceProvider().GetService<TContext>();
        }
    }
}
