namespace Fluffle.Feeder.Bluesky.Core.Domain.Events;

public class BlueskyCreatePostEvent : BlueskyEvent
{
    public required string RKey { get; set; }

    public required string? RootReplyDid { get; set; }

    public required IList<BlueskyImage> Images { get; set; }

    public override T Visit<T>(IBlueskyEventVisitor<T> visitor) => visitor.Visit(this);
}
