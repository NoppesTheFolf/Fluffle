using Microsoft.Extensions.DependencyInjection;

namespace Noppes.Fluffle.Twitter.Database;

public static class ServiceCollectionExtensions
{
    public static void AddTwitterDatabase(this IServiceCollection services, string connectionString, string database)
    {
        services.AddSingleton(new MongoSlowQueryLogger(TimeSpan.FromSeconds(1)));
        services.AddSingleton(x =>
        {
            var context = new TwitterContext(connectionString, database);
            context.AddEventListener(x.GetRequiredService<MongoSlowQueryLogger>());

            return context;
        });
    }
}
