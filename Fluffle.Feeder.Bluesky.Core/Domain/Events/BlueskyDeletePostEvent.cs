namespace Fluffle.Feeder.Bluesky.Core.Domain.Events;

public class BlueskyDeletePostEvent : BlueskyEvent
{
    public required string RKey { get; set; }

    public override T Visit<T>(IBlueskyEventVisitor<T> visitor) => visitor.Visit(this);
}
