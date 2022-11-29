namespace Noppes.Fluffle.KeyValue;

public interface ITypedValueStore<T>
{
    Task<KeyValueResult<T>?> GetAsync();

    Task SetAsync(T value);
}
