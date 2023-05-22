using Microsoft.Extensions.DependencyInjection;

namespace Noppes.Fluffle.Queue.Queuey;

public static class ServiceCollectionExtensions
{
    public static void UseQueuey(this IServiceCollection services, string url, string apiKey)
    {
        services.AddSingleton(new QueueyApiClient(url, apiKey));
        services.AddSingleton<IQueueProvider, QueueyQueueProvider>();
    }
}