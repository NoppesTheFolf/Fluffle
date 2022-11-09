using Azure.Data.Tables;
using Nito.AsyncEx;

namespace Noppes.Fluffle.KeyValue.Azure;

internal class TableClientProvider
{
    private readonly TableServiceClient _serviceClient;
    private readonly IDictionary<string, TableClient> _tableClients;
    private readonly AsyncLock _lock;

    public TableClientProvider(TableServiceClient serviceClient)
    {
        _serviceClient = serviceClient;
        _tableClients = new Dictionary<string, TableClient>(StringComparer.Ordinal);
        _lock = new AsyncLock();
    }

    public async Task<TableClient> GetAsync(string name)
    {
        using var _ = await _lock.LockAsync();
        if (_tableClients.TryGetValue(name, out var tableClient))
            return tableClient;

        tableClient = _serviceClient.GetTableClient(name);
        await tableClient.CreateIfNotExistsAsync();

        _tableClients[name] = tableClient;

        return tableClient;
    }
}