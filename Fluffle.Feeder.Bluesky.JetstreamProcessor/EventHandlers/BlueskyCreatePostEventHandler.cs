using Fluffle.Feeder.Bluesky.Core.Domain;
using Fluffle.Feeder.Bluesky.Core.Domain.Events;
using Fluffle.Feeder.Bluesky.Core.Repositories;
using Fluffle.Feeder.Bluesky.JetstreamProcessor.ApiClient;
using Fluffle.Feeder.Framework.Ingestion;
using Fluffle.Inference.Api.Client;
using Fluffle.Ingestion.Api.Client;
using System.Net;

namespace Fluffle.Feeder.Bluesky.JetstreamProcessor.EventHandlers;

public class BlueskyCreatePostEventHandler : IBlueskyEventHandler
{
    private readonly BlueskyCreatePostEvent _blueskyEvent;
    private readonly IInferenceApiClient _inferenceApiClient;
    private readonly IBlueskyProfileRepository _profileRepository;
    private readonly IBlueskyApiClient _blueskyApiClient;
    private readonly IIngestionApiClient _ingestionApiClient;
    private readonly IBlueskyPostRepository _postRepository;
    private readonly ILogger<BlueskyCreatePostEventHandler> _logger;

    public BlueskyCreatePostEventHandler(BlueskyCreatePostEvent blueskyEvent, IServiceProvider serviceProvider)
    {
        _blueskyEvent = blueskyEvent;
        _inferenceApiClient = serviceProvider.GetRequiredService<IInferenceApiClient>();
        _profileRepository = serviceProvider.GetRequiredService<IBlueskyProfileRepository>();
        _blueskyApiClient = serviceProvider.GetRequiredService<IBlueskyApiClient>();
        _ingestionApiClient = serviceProvider.GetRequiredService<IIngestionApiClient>();
        _postRepository = serviceProvider.GetRequiredService<IBlueskyPostRepository>();
        _logger = serviceProvider.GetRequiredService<ILogger<BlueskyCreatePostEventHandler>>();
    }

    public async Task RunAsync()
    {
        if (_blueskyEvent.RootReplyDid != null && _blueskyEvent.RootReplyDid != _blueskyEvent.Did)
        {
            _logger.LogInformation("Skipping post by {Did} as post is a reply to a different profile.", _blueskyEvent.Did);
            return;
        }

        var profile = await _profileRepository.GetAsync(_blueskyEvent.Did);
        if (profile == null)
        {
            profile = new BlueskyProfile
            {
                Did = _blueskyEvent.Did,
                Handle = null,
                DisplayName = null,
                ImagePredictions = []
            };
            await _profileRepository.CreateAsync(profile);
        }

        if (profile.ImagePredictions.Count >= 25)
        {
            var averagePrediction = profile.ImagePredictions.Average(x => x.Prediction);
            if (averagePrediction < 0.25)
            {
                _logger.LogInformation("Skipping post by {Did} as profile has {Count} images with an average prediction of {Average:P2}.", _blueskyEvent.Did, profile.ImagePredictions.Count, averagePrediction);
                return;
            }
        }

        var imagePredictions = new List<BlueskyImagePrediction>();
        foreach (var image in _blueskyEvent.Images)
        {
            var url = $"https://cdn.bsky.app/img/feed_thumbnail/plain/{_blueskyEvent.Did}/{image.Link}@jpeg";

            float prediction;
            try
            {
                var imageStream = await _blueskyApiClient.GetStreamAsync(url);
                prediction = await _inferenceApiClient.BlueskyFurryArtAsync(imageStream);
            }
            catch (HttpRequestException e)
            {
                if (e.StatusCode == HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("Image at URL {Url} resulted in a 404 Not Found.", url);
                    continue;
                }

                throw;
            }

            imagePredictions.Add(new BlueskyImagePrediction
            {
                Link = image.Link,
                MimeType = image.MimeType,
                Prediction = prediction,
                When = DateTime.UtcNow
            });
        }

        if (imagePredictions.Count == 0)
        {
            return;
        }

        await _profileRepository.AddImagePredictionsAsync(_blueskyEvent.Did, imagePredictions);

        var furryImagePredictions = imagePredictions.Where(x => x.Prediction >= 0.75).ToList();
        if (furryImagePredictions.Count == 0)
        {
            return;
        }

        if (profile.Handle == null)
        {
            BlueskyApiProfile apiProfile;
            try
            {
                apiProfile = await _blueskyApiClient.GetProfileAsync(_blueskyEvent.Did);
            }
            catch (BlueskyApiException e)
            {
                if (e.Error.Message == "Profile not found")
                {
                    _logger.LogWarning("Profile with DID {Did} could not be found.", _blueskyEvent.Did);
                    return;
                }

                throw;
            }

            profile.Handle = apiProfile.Handle;
            profile.DisplayName = apiProfile.DisplayName;

            await _profileRepository.SetHandleAndDisplayNameAsync(_blueskyEvent.Did, apiProfile.Handle, apiProfile.DisplayName);
        }

        var ingestionModelBuilder = new GroupedPutItemActionModelBuilder($"bluesky_{_blueskyEvent.Did}-{_blueskyEvent.RKey}");
        for (var i = 0; i < furryImagePredictions.Count; i++)
        {
            ingestionModelBuilder.AddItem()
                .WithItemId($"bluesky_{_blueskyEvent.Did}-{_blueskyEvent.RKey}-{i}")
                .WithCreatedWhen(DateTimeOffset.FromUnixTimeMilliseconds(_blueskyEvent.UnixTimeMicroseconds / 1000))
                .WithUrl($"https://bsky.app/profile/{_blueskyEvent.Did}/post/{_blueskyEvent.RKey}")
                .WithImage(1000, 1000, $"https://cdn.bsky.app/img/feed_thumbnail/plain/{_blueskyEvent.Did}/{furryImagePredictions[i].Link}@jpeg")
                .SkipImageExtensionValidation()
                .WithAuthor(profile.Did, string.IsNullOrWhiteSpace(profile.DisplayName) ? profile.Handle : profile.DisplayName)
                .WithIsSfw(false);
        }
        var ingestionModels = ingestionModelBuilder.Build();
        await _ingestionApiClient.PutItemActionsAsync(ingestionModels);

        await _postRepository.UpsertAsync(new BlueskyPost
        {
            Id = new BlueskyPostId(_blueskyEvent.Did, _blueskyEvent.RKey),
            UnixTimeMicroseconds = _blueskyEvent.UnixTimeMicroseconds,
            Images = furryImagePredictions
        });
    }
}
