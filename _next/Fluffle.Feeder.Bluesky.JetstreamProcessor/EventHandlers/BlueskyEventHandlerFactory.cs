using Fluffle.Feeder.Bluesky.Core.Domain.Events;

namespace Fluffle.Feeder.Bluesky.JetstreamProcessor.EventHandlers;

public class BlueskyEventHandlerFactory : IBlueskyEventVisitor<IBlueskyEventHandler>
{
    private readonly IServiceProvider _serviceProvider;

    public BlueskyEventHandlerFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IBlueskyEventHandler Visit(BlueskyCreatePostEvent blueskyEvent) =>
        new BlueskyCreatePostEventHandler(blueskyEvent, _serviceProvider);

    public IBlueskyEventHandler Visit(BlueskyDeletePostEvent blueskyEvent) =>
        new BlueskyDeletePostEventHandler(blueskyEvent, _serviceProvider);

    public IBlueskyEventHandler Visit(BlueskyDeleteAccountEvent blueskyEvent) =>
        new BlueskyDeleteAccountEventHandler(blueskyEvent, _serviceProvider);
}
