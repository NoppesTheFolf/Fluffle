using Noppes.Fluffle.KeyValue;

namespace Noppes.Fluffle.DeviantArt.Shared;

public interface IQueryDeviationsLatestPublishedWhenStore : ITypedValueStore<IDictionary<string, DateTimeOffset>>
{
}

public class QueryDeviationsLatestPublishedWhenStore : IQueryDeviationsLatestPublishedWhenStore
{
    private const string Name = "queryDeviationsLatestPublishedWhen";
    private const string Key = "null";

    private readonly IKeyValueStore _keyValueStore;

    public QueryDeviationsLatestPublishedWhenStore(IKeyValueStore keyValueStore)
    {
        _keyValueStore = keyValueStore;
    }

    public Task<KeyValueResult<IDictionary<string, DateTimeOffset>>?> GetAsync() => _keyValueStore.GetAsync<IDictionary<string, DateTimeOffset>>(Name, Key);

    public Task SetAsync(IDictionary<string, DateTimeOffset> value) => _keyValueStore.SetAsync(Name, Key, value);
}
