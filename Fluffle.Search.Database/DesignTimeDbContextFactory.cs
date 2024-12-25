using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;
using Noppes.Fluffle.Configuration;

namespace Noppes.Fluffle.Search.Database;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<FluffleSearchContext>
{
    public FluffleSearchContext CreateDbContext(string[] args)
    {
        var services = new ServiceCollection();
        var conf = FluffleConfiguration.Load<FluffleSearchContext>().Get<SearchDatabaseConfiguration>();

        services.AddDbContext<FluffleSearchContext>(dbContextOptions =>
        {
            dbContextOptions.UseNpgsql(conf.ConnectionString, npgsqlOptions =>
            {
                npgsqlOptions.CommandTimeout(10 * 60);
            });
        });

        return services.BuildServiceProvider().GetService<FluffleSearchContext>();
    }
}
