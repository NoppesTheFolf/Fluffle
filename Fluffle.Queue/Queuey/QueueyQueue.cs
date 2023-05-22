using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Noppes.Fluffle.Queue.Queuey;

internal class QueueyQueue
{
    public static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
        }
    };
}

internal class QueueyQueue<T> : IQueue<T>
{
    private readonly string _name;
    private readonly QueueyApiClient _apiClient;

    public QueueyQueue(string name, QueueyApiClient apiClient)
    {
        _name = name;
        _apiClient = apiClient;
    }

    public async Task EnqueueAsync(T? value, int? priority, TimeSpan? visibleAfter, TimeSpan? expireAfter)
    {
        await EnqueueManyAsync(new[] { value }, priority, visibleAfter, expireAfter);
    }

    public async Task EnqueueManyAsync(IEnumerable<T?> values, int? priority, TimeSpan? visibleAfter, TimeSpan? expireAfter)
    {
        var items = values.Select(x =>
        {
            var utf8Bytes = JsonSerializer.SerializeToUtf8Bytes(x, QueueyQueue.JsonSerializerOptions);
            var message = Encoding.UTF8.GetString(utf8Bytes);

            return new QueueyEnqueueItem
            {
                Priority = priority,
                Message = message,
                VisibleWhen = visibleAfter == null ? DateTime.UtcNow : DateTime.UtcNow.Add((TimeSpan)visibleAfter)
            };
        }).ToList();

        await _apiClient.EnqueueAsync(_name, items);
    }

    public async Task<QueueItem<T?>?> DequeueAsync(TimeSpan visibleAfter)
    {
        var items = await DequeueManyAsync(visibleAfter, 1);
        if (!items.Any())
            return null;

        var item = items.First();
        return item;
    }

    public async Task<ICollection<QueueItem<T?>>> DequeueManyAsync(TimeSpan visibleAfter, int? limit = null)
    {
        var models = await _apiClient.DequeueAsync(_name, limit ?? 32, visibleAfter);
        if (!models.Any())
            return Array.Empty<QueueItem<T?>>();

        var items = models.Select(x =>
        {
            var value = JsonSerializer.Deserialize<T>(x.Message, QueueyQueue.JsonSerializerOptions);

            return (QueueItem<T?>)new QueueyQueueItem<T?>(value, _name, x.Id, _apiClient);
        }).ToList();
        return items;
    }
}
