using Microsoft.Extensions.DependencyInjection;

namespace Noppes.Fluffle.KeyValue;

public static class ServiceCollectionExtensions
{
    public static void AddKeyValueStore(this IServiceCollection services)
    {
        services.AddSingleton(x =>
        {
            var keyValueStoreProvider = x.GetRequiredService<IKeyValueStoreProvider>();
            var keyValueStore = keyValueStoreProvider.Get();

            return keyValueStore;
        });
    }
}
