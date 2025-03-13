using Noppes.Fluffle.FurAffinity.Models;
using System;
using System.Threading.Tasks;

namespace Noppes.Fluffle.FurAffinitySync;

public enum StopReason
{
    Id,
    Time
}

public enum Direction
{
    Forward,
    Backward
}

public class SequentialSubmissionRetriever
{
    private int _id;
    private readonly Direction _direction;
    private readonly TimeSpan? _stopTime;
    private readonly int? _stopId;
    private readonly GetSubmissionScheduler _getSubmissionScheduler;
    private readonly int _priority;

    public SequentialSubmissionRetriever(int id, Direction direction, TimeSpan? stopTime, int? stopId, GetSubmissionScheduler getSubmissionScheduler, int priority)
    {
        if (direction == Direction.Backward && _stopTime != null)
            throw new InvalidOperationException("Direction backward with stop time not supported.");

        _id = id;
        _direction = direction;
        _stopTime = stopTime;
        _stopId = stopId;
        _getSubmissionScheduler = getSubmissionScheduler;
        _priority = priority;
    }

    public async Task<(StopReason?, int, FaResult<FaSubmission>)> NextAsync()
    {
        var id = _direction == Direction.Forward ? ++_id : --_id;
        if ((_direction == Direction.Forward && id >= _stopId) || (_direction == Direction.Backward && id <= _stopId))
            return (StopReason.Id, id, null);

        var faResult = await _getSubmissionScheduler.ProcessAsync(new GetSubmissionSchedulerItem
        {
            SubmissionId = id
        }, _priority);

        if (faResult == null)
            return (null, id, null);

        if (_stopTime != null && faResult.Result.When > DateTimeOffset.UtcNow.Subtract((TimeSpan)_stopTime))
            return (StopReason.Time, id, faResult);

        return (null, id, faResult);
    }
}
