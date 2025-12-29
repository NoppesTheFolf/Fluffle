namespace Fluffle.Feeder.Bluesky.Core.Domain.Events;

public class BlueskyDeleteAccountEvent : BlueskyEvent
{
    public override T Visit<T>(IBlueskyEventVisitor<T> visitor) => visitor.Visit(this);
}
