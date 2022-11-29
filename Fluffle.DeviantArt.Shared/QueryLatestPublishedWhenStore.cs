using Noppes.Fluffle.KeyValue;

namespace Noppes.Fluffle.DeviantArt.Shared;

public interface IQueryDeviationsLatestPublishedWhenStore : ITypedKeyValueStore<DateTimeOffset>
{
}

public class QueryDeviationsLatestPublishedWhenStore : IQueryDeviationsLatestPublishedWhenStore
{
    private const string Name = "queryDeviationsLatestPublishedWhen";

    private readonly IKeyValueStore _keyValueStore;

    public QueryDeviationsLatestPublishedWhenStore(IKeyValueStore keyValueStore)
    {
        _keyValueStore = keyValueStore;
    }

    public Task<KeyValueResult<DateTimeOffset>?> GetAsync(string key) => _keyValueStore.GetAsync<DateTimeOffset>(Name, key);

    public Task SetAsync(string key, DateTimeOffset value) => _keyValueStore.SetAsync(Name, key, value);
}
