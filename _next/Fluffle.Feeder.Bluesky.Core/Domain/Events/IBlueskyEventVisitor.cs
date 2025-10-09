namespace Fluffle.Feeder.Bluesky.Core.Domain.Events;

public interface IBlueskyEventVisitor<out T>
{
    T Visit(BlueskyCreatePostEvent blueskyEvent);

    T Visit(BlueskyDeletePostEvent blueskyEvent);

    T Visit(BlueskyDeleteAccountEvent blueskyEvent);
}
