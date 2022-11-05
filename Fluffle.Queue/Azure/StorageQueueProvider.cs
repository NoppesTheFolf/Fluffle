namespace Noppes.Fluffle.Queue.Azure;

internal class StorageQueueProvider : IQueueProvider
{
    private readonly IQueueClientProvider _queueClientProvider;

    public StorageQueueProvider(IQueueClientProvider queueClientProvider)
    {
        _queueClientProvider = queueClientProvider;
    }

    public IQueue<T> Get<T>(string name)
    {
        var queue = _queueClientProvider.Get(name);

        return new StorageQueue<T>(queue);
    }
}