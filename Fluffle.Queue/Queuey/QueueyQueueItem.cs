namespace Noppes.Fluffle.Queue.Queuey;

internal class QueueyQueueItem<T> : QueueItem<T>
{
    private readonly string _name; // Name of the queue
    private readonly string _id; // ID of the queue item
    private readonly QueueyApiClient _apiClient;

    public QueueyQueueItem(T value, string name, string id, QueueyApiClient apiClient) : base(value)
    {
        _name = name;
        _id = id;
        _apiClient = apiClient;
    }

    public override async Task AcknowledgeAsync()
    {
        await _apiClient.AcknowledgeAsync(_name, _id);
    }
}
