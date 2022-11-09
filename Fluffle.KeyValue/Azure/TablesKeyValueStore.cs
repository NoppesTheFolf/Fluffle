using System.Text.Json;
using System.Text.Json.Serialization;

namespace Noppes.Fluffle.KeyValue.Azure;

internal class TablesKeyValueStore : IKeyValueStore
{
    private const string PartitionKey = "null";
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
        }
    };

    private readonly TableClientProvider _tableClientProvider;

    public TablesKeyValueStore(TableClientProvider tableClientProvider)
    {
        _tableClientProvider = tableClientProvider;
    }

    public async Task<KeyValueResult<T>?> GetAsync<T>(string name, string key)
    {
        var client = await _tableClientProvider.GetAsync(name);

        var response = await client.GetEntityIfExistsAsync<TablesKeyValueEntity>(PartitionKey, key);
        if (!response.HasValue)
            return null;

        var data = response.Value.Value;
        var value = JsonSerializer.Deserialize<T>(data, JsonSerializerOptions);

        return new KeyValueResult<T>(value);
    }

    public async Task SetAsync<T>(string name, string key, T value)
    {
        var client = await _tableClientProvider.GetAsync(name);

        var data = JsonSerializer.Serialize(value, JsonSerializerOptions);
        await client.UpsertEntityAsync(new TablesKeyValueEntity
        {
            PartitionKey = PartitionKey,
            RowKey = key,
            Value = data
        });
    }
}