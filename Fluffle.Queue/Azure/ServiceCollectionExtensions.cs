using Microsoft.Extensions.DependencyInjection;

namespace Noppes.Fluffle.Queue.Azure;

public static class ServiceCollectionExtensions
{
    public static void UseStorageQueue(this IServiceCollection services, string connectionString)
    {
        services.AddSingleton<IQueueClientProvider>(new QueueClientProvider(connectionString));
        services.AddSingleton<IQueueProvider, StorageQueueProvider>();
    }
}
