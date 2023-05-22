using Microsoft.Extensions.DependencyInjection;

namespace Noppes.Fluffle.Twitter.Database;

public static class ServiceCollectionExtensions
{
    public static void AddTwitterDatabase(this IServiceCollection services, string connectionString, string database)
    {
        services.AddSingleton(new TwitterContext(connectionString, database));
    }
}