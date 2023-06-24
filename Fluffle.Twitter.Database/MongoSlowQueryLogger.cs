using MongoDB.Driver.Core.Events;
using Serilog;

namespace Noppes.Fluffle.Twitter.Database;

public class MongoSlowQueryLogger : IMongoEventListener
{
    private readonly object _lock = new();
    private readonly IDictionary<int, string> _commands;
    private readonly IDictionary<int, TimeSpan> _commandDurations;

    private readonly TimeSpan _slowThreshold;

    public MongoSlowQueryLogger(TimeSpan slowThreshold)
    {
        _commands = new Dictionary<int, string>();
        _commandDurations = new Dictionary<int, TimeSpan>();
        _slowThreshold = slowThreshold;
    }

    public void Handle(CommandStartedEvent commandStartedEvent)
    {
        lock (_lock)
        {
            _commands[commandStartedEvent.RequestId] = commandStartedEvent.Command.ToString();
            Update(commandStartedEvent.RequestId);
        }
    }

    public void Handle(CommandSucceededEvent commandSucceededEvent) => HandleCommandFinished(commandSucceededEvent.RequestId, commandSucceededEvent.Duration);

    public void Handle(CommandFailedEvent commandFailedEvent) => HandleCommandFinished(commandFailedEvent.RequestId, commandFailedEvent.Duration);

    public void HandleCommandFinished(int requestId, TimeSpan duration)
    {
        lock (_lock)
        {
            _commandDurations[requestId] = duration;
            Update(requestId);
        }
    }

    private void Update(int requestId)
    {
        if (!_commands.TryGetValue(requestId, out var commandStartedEvent))
            return;

        if (!_commandDurations.TryGetValue(requestId, out var duration))
            return;

        _commands.Remove(requestId);
        _commandDurations.Remove(requestId);

        if (duration < _slowThreshold)
            return;

        Log.Warning("Query took {duration} to execute: {command}", $"{Math.Ceiling(duration.TotalMilliseconds)}ms", commandStartedEvent);
    }
}
