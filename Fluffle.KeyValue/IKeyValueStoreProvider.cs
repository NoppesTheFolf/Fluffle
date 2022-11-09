namespace Noppes.Fluffle.KeyValue;

public interface IKeyValueStoreProvider
{
    IKeyValueStore Get();
}