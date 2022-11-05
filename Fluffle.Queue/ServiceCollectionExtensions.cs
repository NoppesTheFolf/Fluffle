using Microsoft.Extensions.DependencyInjection;

namespace Noppes.Fluffle.Queue;

public static class ServiceCollectionExtensions
{
    public static void AddQueue<T>(this IServiceCollection services, string name)
    {
        services.AddSingleton(x =>
        {
            var queueProvider = x.GetRequiredService<IQueueProvider>();
            var queue = queueProvider.Get<T>(name);

            return queue;
        });
    }
}
