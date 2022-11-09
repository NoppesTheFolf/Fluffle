using Azure.Data.Tables;
using Microsoft.Extensions.DependencyInjection;

namespace Noppes.Fluffle.KeyValue.Azure;

public static class ServiceCollectionExtensions
{
    public static void UseTableStorage(this IServiceCollection services, string connectionString)
    {
        services.AddSingleton(_ =>
        {
            var serviceClient = new TableServiceClient(connectionString);

            return new TableClientProvider(serviceClient);
        });
        services.AddSingleton<IKeyValueStoreProvider, TablesKeyValueStoreProvider>();
    }
}
