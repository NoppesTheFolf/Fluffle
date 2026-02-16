using Fluffle.Feeder.Bluesky.Core.Domain;
using Fluffle.Feeder.Bluesky.Core.Domain.Events;
using Fluffle.Feeder.Bluesky.Core.Repositories;
using Fluffle.Feeder.Framework.StatePersistence;
using Microsoft.Extensions.Options;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Fluffle.Feeder.Bluesky.JetstreamWatcher;

public class Worker : BackgroundService
{
    private readonly IStateRepository<JetstreamWatcherState> _stateRepository;
    private readonly IBlueskyEventRepository _eventRepository;
    private readonly IOptions<BlueskyJetstreamWatcherOptions> _options;
    private readonly ILogger<Worker> _logger;

    public Worker(
        IStateRepositoryFactory stateRepositoryFactory,
        IBlueskyEventRepository eventRepository,
        IOptions<BlueskyJetstreamWatcherOptions> options,
        ILogger<Worker> logger)
    {
        _stateRepository = stateRepositoryFactory.Create<JetstreamWatcherState>("BlueskyJetstreamWatcher");
        _eventRepository = eventRepository;
        _options = options;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var state = await _stateRepository.GetAsync() ?? new JetstreamWatcherState
        {
            UnixTimeMicroseconds = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() * 1000
        };
        var cursor = state.UnixTimeMicroseconds - (long)TimeSpan.FromSeconds(2).TotalMicroseconds;

        _logger.LogInformation("Start connecting to {InstanceHostname}.", _options.Value.InstanceHostname);
        using var webSocket = new ClientWebSocket();
        await webSocket.ConnectAsync(new Uri($"wss://{_options.Value.InstanceHostname}/subscribe?cursor={cursor}&wantedCollections=app.bsky.feed.post"), stoppingToken);
        _logger.LogInformation("Connection has been established.");

        try
        {
            using var messageBuffer = new MemoryStream();
            var receiveBuffer = new byte[8192].AsMemory();
            while (!stoppingToken.IsCancellationRequested)
            {
                var receiveResult = await webSocket.ReceiveAsync(receiveBuffer, stoppingToken);

                messageBuffer.Write(receiveBuffer[..receiveResult.Count].Span);

                if (!receiveResult.EndOfMessage)
                {
                    continue;
                }

                messageBuffer.Position = 0;
                var message = Encoding.UTF8.GetString(messageBuffer.ToArray());
                messageBuffer.SetLength(0);

                JsonNode messageNode;
                try
                {
                    messageNode = JsonNode.Parse(message)!;
                }
                catch (JsonException)
                {
                    _logger.LogWarning($"A {nameof(JsonException)} occurred while trying to parse a message as JSON.");
                    continue;
                }

                await HandleMessage(messageNode);

                var unixTimeMicroseconds = messageNode["time_us"]!.GetValue<long>();
                var elapsed = TimeSpan.FromMicroseconds(unixTimeMicroseconds - state.UnixTimeMicroseconds);
                if (elapsed > TimeSpan.FromSeconds(15))
                {
                    state.UnixTimeMicroseconds = unixTimeMicroseconds;
                    await _stateRepository.PutAsync(state);
                    _logger.LogInformation("Saved cursor at {UnixTimeMicroseconds} µs.", state.UnixTimeMicroseconds);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // ignore
        }

        if (webSocket.State is WebSocketState.Open or WebSocketState.CloseReceived or WebSocketState.CloseSent)
        {
            _logger.LogInformation("Start closing websocket.");
            using var closingCts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, closingCts.Token);
            _logger.LogInformation("Websocket has been closed.");
        }
    }

    private async Task HandleMessage(JsonNode messageNode)
    {
        var messageKind = messageNode["kind"]!.GetValue<string>();
        Func<Task> handleMessage = messageKind switch
        {
            "account" => () => HandleAccountAsync(messageNode),
            "commit" => () => HandleCommit(messageNode),
            _ => () => Task.CompletedTask
        };
        await handleMessage();
    }

    private async Task HandleAccountAsync(JsonNode messageNode)
    {
        var accountNode = messageNode["account"]!;
        var accountStatus = accountNode["status"]?.GetValue<string>();
        if (accountStatus != "deleted")
        {
            return;
        }

        var did = messageNode["did"]!.GetValue<string>();
        var unixTime = messageNode["time_us"]!.GetValue<long>();
        await _eventRepository.CreateAsync(new BlueskyDeleteAccountEvent
        {
            Did = did,
            UnixTimeMicroseconds = unixTime
        });
    }

    private async Task HandleCommit(JsonNode messageNode)
    {
        var commitNode = messageNode["commit"]!;
        var operation = commitNode["operation"]!.GetValue<string>();

        Func<Task> handleCommit = operation switch
        {
            "delete" => () => HandleCommitDeleteAsync(messageNode, commitNode),
            "create" or "update" => () => HandleCommitCreateOrUpdateAsync(messageNode, commitNode),
            _ => () => Task.CompletedTask
        };
        await handleCommit();
    }

    private async Task HandleCommitDeleteAsync(JsonNode messageNode, JsonNode commitNode)
    {
        var did = messageNode["did"]!.GetValue<string>();
        var unixTime = messageNode["time_us"]!.GetValue<long>();
        var rkey = commitNode["rkey"]!.GetValue<string>();
        await _eventRepository.CreateAsync(new BlueskyDeletePostEvent
        {
            Did = did,
            RKey = rkey,
            UnixTimeMicroseconds = unixTime
        });
    }

    private async Task HandleCommitCreateOrUpdateAsync(JsonNode messageNode, JsonNode commitNode)
    {
        var recordNode = commitNode["record"]!;

        string? rootReplyDid = null;
        var replyNode = recordNode["reply"];
        if (replyNode != null)
        {
            var rootReplyUri = replyNode["root"]!["uri"]!.GetValue<string>();
            rootReplyDid = rootReplyUri.Split('/')[2];
        }

        var embedNode = recordNode["embed"];
        if (embedNode == null)
        {
            return;
        }

        var embedType = embedNode["$type"]!.GetValue<string>();
        if (embedType is "app.bsky.embed.external" or "app.bsky.embed.record" or "app.bsky.embed.video")
        {
            return;
        }

        var images = embedType switch
        {
            "app.bsky.embed.images" => HandleEmbedImages(embedNode),
            "app.bsky.embed.recordWithMedia" => HandleEmbedRecordWithMedia(embedNode),
            _ => null
        };

        if (images == null)
        {
            _logger.LogWarning("Unhandled embed type: {EmbedType}.", embedType);
            return;
        }

        if (images.Count == 0)
        {
            return;
        }

        var did = messageNode["did"]!.GetValue<string>();
        var unixTime = messageNode["time_us"]!.GetValue<long>();
        var rkey = commitNode["rkey"]!.GetValue<string>();
        await _eventRepository.CreateAsync(new BlueskyCreatePostEvent
        {
            UnixTimeMicroseconds = unixTime,
            Did = did,
            RKey = rkey,
            RootReplyDid = rootReplyDid,
            Images = images
        });
    }

    private static IList<BlueskyImage> HandleEmbedImages(JsonNode embedNode)
    {
        var images = ExtractImages(embedNode["images"]!);
        return images;
    }

    private static IList<BlueskyImage> HandleEmbedRecordWithMedia(JsonNode embedNode)
    {
        var mediaNode = embedNode["media"]!;
        var mediaType = mediaNode["$type"]!.GetValue<string>();
        if (mediaType != "app.bsky.embed.images")
        {
            return Array.Empty<BlueskyImage>();
        }

        var images = ExtractImages(mediaNode["images"]!);
        return images;
    }

    private static IList<BlueskyImage> ExtractImages(JsonNode imagesNode)
    {
        var images = new List<BlueskyImage>();
        foreach (var imageContainerNode in imagesNode.AsArray())
        {
            var imageNode = imageContainerNode!["image"]!;

            var imageType = imageNode["$type"]?.GetValue<string>();
            var link = imageType == "blob"
                ? imageNode["ref"]!["$link"]!.GetValue<string>()
                : imageNode["cid"]!.GetValue<string>();
            var mimeType = imageNode["mimeType"]!.GetValue<string>();
            images.Add(new BlueskyImage
            {
                Link = link,
                MimeType = mimeType
            });
        }

        return images;
    }
}
