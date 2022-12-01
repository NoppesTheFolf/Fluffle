using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Noppes.Fluffle.Queue.Azure;

internal class StorageQueue<T> : IQueue<T>
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
        }
    };

    private readonly QueueClient _queueClient;

    public StorageQueue(QueueClient queueClient)
    {
        _queueClient = queueClient;
    }

    public async Task EnqueueAsync(T? value, TimeSpan? visibleAfter, TimeSpan? expireAfter)
    {
        var body = JsonSerializer.SerializeToUtf8Bytes(value, JsonSerializerOptions);
        var data = new BinaryData(body);

        var timeToLive = expireAfter ?? TimeSpan.FromSeconds(-1);
        await _queueClient.SendMessageAsync(data, visibleAfter, timeToLive);
    }

    public async Task EnqueueManyAsync(IEnumerable<T?> values, TimeSpan? visibleAfter, TimeSpan? expireAfter)
    {
        foreach (var value in values)
            await EnqueueAsync(value, visibleAfter, expireAfter);
    }

    public async Task<QueueItem<T?>?> DequeueAsync()
    {
        var response = await _queueClient.ReceiveMessageAsync();
        var message = response.Value;

        if (message == null)
            return null;

        var item = MessageToQueueItem(message);
        return item;
    }

    private StorageQueueItem<T?> MessageToQueueItem(QueueMessage message)
    {
        var body = message.Body.ToArray();
        var value = JsonSerializer.Deserialize<T>(body, JsonSerializerOptions);
        var item = new StorageQueueItem<T?>(value, _queueClient, message.MessageId, message.PopReceipt);

        return item;
    }

    public async Task<ICollection<QueueItem<T?>>> DequeueManyAsync(int? limit = null)
    {
        var remaining = limit ?? _queueClient.MaxPeekableMessages;

        var items = new List<QueueItem<T?>>();
        while (true)
        {
            var maxMessages = Math.Min(remaining, _queueClient.MaxPeekableMessages);
            var response = await _queueClient.ReceiveMessagesAsync(maxMessages);
            var messages = response.Value;

            items.AddRange(messages.Select(MessageToQueueItem));

            remaining -= messages.Length;
            if (remaining <= 0 || messages.Length != _queueClient.MaxPeekableMessages)
                break;
        }

        return items;
    }
}