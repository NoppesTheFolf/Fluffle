namespace Noppes.Fluffle.KeyValue;

public interface ITypedKeyValueStore<T>
{
    Task<KeyValueResult<T>?> GetAsync(string key);

    Task SetAsync(string key, T value);
}
