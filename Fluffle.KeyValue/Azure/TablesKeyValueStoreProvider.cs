namespace Noppes.Fluffle.KeyValue.Azure;

internal class TablesKeyValueStoreProvider : IKeyValueStoreProvider
{
    private readonly TableClientProvider _tableClientProvider;

    public TablesKeyValueStoreProvider(TableClientProvider tableClientProvider)
    {
        _tableClientProvider = tableClientProvider;
    }

    public IKeyValueStore Get()
    {
        return new TablesKeyValueStore(_tableClientProvider);
    }
}