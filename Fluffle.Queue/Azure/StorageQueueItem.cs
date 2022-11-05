using Azure.Storage.Queues;

namespace Noppes.Fluffle.Queue.Azure;

public class StorageQueueItem<T> : QueueItem<T>
{
    private readonly QueueClient _queueClient;
    private readonly string _messageId;
    private readonly string _popReceipt;

    public StorageQueueItem(T value, QueueClient queueClient, string messageId, string popReceipt) : base(value)
    {
        _queueClient = queueClient;
        _messageId = messageId;
        _popReceipt = popReceipt;
    }

    public override async Task AcknowledgeAsync()
    {
        await _queueClient.DeleteMessageAsync(_messageId, _popReceipt);
    }
}