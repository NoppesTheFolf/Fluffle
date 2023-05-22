using Flurl.Http;
using Noppes.Fluffle.Http;

namespace Noppes.Fluffle.Queue.Queuey;

internal class QueueyEnqueueItem
{
    public int? Priority { get; set; }

    public string Message { get; set; } = null!;

    public DateTime? VisibleWhen { get; set; }
}

internal class QueueyDequeueItem
{
    public string Id { get; set; } = null!;

    public string Message { get; set; } = null!;
}

internal class QueueyApiClient : ApiClient
{
    private readonly string _apiKey;

    public QueueyApiClient(string baseUrl, string apiKey) : base(baseUrl)
    {
        _apiKey = apiKey;
    }

    public async Task AcknowledgeAsync(string name, string id)
    {
        await Request("queue", name, id, "acknowledge")
            .GetAsync();
    }

    public async Task<IList<QueueyDequeueItem>> DequeueAsync(string name, int limit = 32, TimeSpan? visibilityDelay = null)
    {
        var items = await Request("queue", name, "dequeue")
            .SetQueryParam("limit", limit)
            .SetQueryParam("visibilityDelay", visibilityDelay)
            .AcceptJson().GetJsonAsync<IList<QueueyDequeueItem>>();

        return items;
    }

    public async Task EnqueueAsync(string name, ICollection<QueueyEnqueueItem> items)
    {
        await Request("queue", name, "enqueue")
            .PostJsonAsync(items);
    }

    public override IFlurlRequest Request(params object[] urlSegments)
    {
        return base.Request(urlSegments)
            .WithHeader("Api-Key", _apiKey);
    }
}
