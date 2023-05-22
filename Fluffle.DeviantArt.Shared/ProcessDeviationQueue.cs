using Humanizer;
using Noppes.Fluffle.DeviantArt.Client.Models;
using Noppes.Fluffle.Queue;

namespace Noppes.Fluffle.DeviantArt.Shared;

public class ProcessDeviationQueueItem
{
    public string Id { get; set; } = null!;
}

public class ProcessDeviationQueue
{
    private static readonly TimeSpan RequiredAge = 15.Minutes();

    private readonly IQueue<ProcessDeviationQueueItem> _queue;

    public ProcessDeviationQueue(IQueue<ProcessDeviationQueueItem> queue)
    {
        _queue = queue;
    }

    public async Task EnqueueManyAsync(IEnumerable<Deviation> deviations)
    {
        foreach (var deviation in deviations)
            await EnqueueAsync(deviation);
    }

    public async Task EnqueueAsync(Deviation deviation)
    {
        var age = DateTime.UtcNow.Subtract(deviation.PublishedWhen.UtcDateTime);
        TimeSpan? visibleAfter = RequiredAge.Subtract(age);
        if (visibleAfter < TimeSpan.Zero)
            visibleAfter = null;

        await _queue.EnqueueAsync(new ProcessDeviationQueueItem
        {
            Id = deviation.Id
        }, null, visibleAfter, null);
    }
}
