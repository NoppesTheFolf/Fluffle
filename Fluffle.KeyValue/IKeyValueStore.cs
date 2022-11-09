namespace Noppes.Fluffle.KeyValue;

public interface IKeyValueStore
{
    Task<KeyValueResult<T>?> GetAsync<T>(string name, string key);

    Task SetAsync<T>(string name, string key, T value);
}