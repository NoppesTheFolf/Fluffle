using Noppes.Fluffle.KeyValue;

namespace Noppes.Fluffle.DeviantArt.Shared;

public interface INewestDeviationsLatestPublishedWhenStore : ITypedValueStore<DateTimeOffset>
{
}

public class NewestDeviationsLatestPublishedWhenStore : INewestDeviationsLatestPublishedWhenStore
{
    private const string Name = "newestDeviationsLatestPublishedWhen";
    private const string Key = "null";

    private readonly IKeyValueStore _keyValueStore;

    public NewestDeviationsLatestPublishedWhenStore(IKeyValueStore keyValueStore)
    {
        _keyValueStore = keyValueStore;
    }

    public Task<KeyValueResult<DateTimeOffset>?> GetAsync() => _keyValueStore.GetAsync<DateTimeOffset>(Name, Key);

    public Task SetAsync(DateTimeOffset value) => _keyValueStore.SetAsync(Name, Key, value);
}
