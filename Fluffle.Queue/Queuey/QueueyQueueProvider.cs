namespace Noppes.Fluffle.Queue.Queuey;

internal class QueueyQueueProvider : IQueueProvider
{
    private readonly QueueyApiClient _client;

    public QueueyQueueProvider(QueueyApiClient client)
    {
        _client = client;
    }

    public IQueue<T> Get<T>(string name)
    {
        return new QueueyQueue<T>(name, _client);
    }
}