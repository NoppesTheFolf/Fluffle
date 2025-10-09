namespace Fluffle.Feeder.Bluesky.Core.Domain.Events;

public abstract class BlueskyEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public required string Did { get; set; }

    public required long UnixTimeMicroseconds { get; set; }

    public int AttemptCount { get; set; } = 0;

    public DateTime VisibleWhen { get; set; } = DateTime.UtcNow;

    public abstract T Visit<T>(IBlueskyEventVisitor<T> visitor);
}
