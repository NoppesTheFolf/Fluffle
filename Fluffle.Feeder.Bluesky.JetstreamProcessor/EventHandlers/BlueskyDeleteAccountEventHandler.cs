using Fluffle.Feeder.Bluesky.Core.Domain.Events;
using Fluffle.Feeder.Bluesky.Core.Repositories;
using Fluffle.Feeder.Framework.Ingestion;
using Fluffle.Ingestion.Api.Client;
using Fluffle.Ingestion.Api.Models.ItemActions;

namespace Fluffle.Feeder.Bluesky.JetstreamProcessor.EventHandlers;

public class BlueskyDeleteAccountEventHandler : IBlueskyEventHandler
{
    private readonly BlueskyDeleteAccountEvent _blueskyEvent;
    private readonly IBlueskyPostRepository _postRepository;
    private readonly IBlueskyProfileRepository _profileRepository;
    private readonly IIngestionApiClient _ingestionApiClient;

    public BlueskyDeleteAccountEventHandler(BlueskyDeleteAccountEvent blueskyEvent, IServiceProvider serviceProvider)
    {
        _blueskyEvent = blueskyEvent;
        _postRepository = serviceProvider.GetRequiredService<IBlueskyPostRepository>();
        _profileRepository = serviceProvider.GetRequiredService<IBlueskyProfileRepository>();
        _ingestionApiClient = serviceProvider.GetRequiredService<IIngestionApiClient>();
    }

    public async Task RunAsync()
    {
        var posts = await _postRepository.GetByDidAsync(_blueskyEvent.Did);
        if (posts.Count > 0)
        {
            var models = posts
                .Select(PutItemActionModel (x) => new PutDeleteGroupItemActionModelBuilder()
                    .WithGroupId($"bluesky_{x.Id.Did}-{x.Id.RKey}")
                    .Build())
                .ToList();

            await _ingestionApiClient.PutItemActionsAsync(models);
        }

        await _postRepository.DeleteByDidAsync(_blueskyEvent.Did);
        await _profileRepository.DeleteAsync(_blueskyEvent.Did);
    }
}
