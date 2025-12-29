using Fluffle.Feeder.Bluesky.Core.Domain.Events;
using Fluffle.Feeder.Bluesky.Core.Repositories;
using Fluffle.Feeder.Framework.Ingestion;
using Fluffle.Ingestion.Api.Client;

namespace Fluffle.Feeder.Bluesky.JetstreamProcessor.EventHandlers;

public class BlueskyDeletePostEventHandler : IBlueskyEventHandler
{
    private readonly BlueskyDeletePostEvent _blueskyEvent;
    private readonly IBlueskyPostRepository _postRepository;
    private readonly IIngestionApiClient _ingestionApiClient;

    public BlueskyDeletePostEventHandler(BlueskyDeletePostEvent blueskyEvent, IServiceProvider serviceProvider)
    {
        _blueskyEvent = blueskyEvent;
        _postRepository = serviceProvider.GetRequiredService<IBlueskyPostRepository>();
        _ingestionApiClient = serviceProvider.GetRequiredService<IIngestionApiClient>();
    }

    public async Task RunAsync()
    {
        var post = await _postRepository.GetAsync(_blueskyEvent.Did, _blueskyEvent.RKey);
        if (post == null)
        {
            return;
        }

        var model = new PutDeleteGroupItemActionModelBuilder().WithGroupId($"bluesky_{post.Id.Did}-{post.Id.RKey}").Build();
        await _ingestionApiClient.PutItemActionsAsync([model]);

        await _postRepository.DeleteAsync(post.Id.Did, post.Id.RKey);
    }
}
