using Humanizer;
using Microsoft.Extensions.Logging;
using Nitranium.PerceptualHashing.Utils;
using Noppes.Fluffle.B2;
using Noppes.Fluffle.Bot.Database;
using Noppes.Fluffle.Configuration;
using Noppes.Fluffle.Constants;
using Noppes.Fluffle.Thumbnail;
using Noppes.Fluffle.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace Noppes.Fluffle.Bot.Utils
{
    public class MediaGroupHandler
    {
        private static readonly TimeSpan ReverseSearchTimeout = 120.Seconds();
        public const int ThumbnailTargetSize = 350;
        private const int ThumbnailQuality = 75;

        private static readonly JsonSerializerOptions JsonSerializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        };

        private readonly BotConfiguration _botConfiguration;
        private readonly ITelegramBotClient _botClient;
        private readonly BotContext _botContext;
        private readonly UploadManagerCollection _uploadManagerCollection;
        private readonly FluffleThumbnail _fluffleThumbnail;
        private readonly ILogger<MediaGroupHandler> _logger;

        public MediaGroupHandler(BotConfiguration botConfiguration, ITelegramBotClient botClient, BotContext botContext, UploadManagerCollection uploadManagerCollection, FluffleThumbnail fluffleThumbnail, ILogger<MediaGroupHandler> logger)
        {
            _botConfiguration = botConfiguration;
            _botClient = botClient;
            _botContext = botContext;
            _uploadManagerCollection = uploadManagerCollection;
            _fluffleThumbnail = fluffleThumbnail;
            _logger = logger;
        }

        private async Task UploadIndex<T>(MediaGroupData mediaGroupData, T data)
        {
            var json = JsonSerializer.Serialize(data, JsonSerializerOptions);
            var buffer = Encoding.UTF8.GetBytes(json);
            await _uploadManagerCollection.Index.ProcessAsync(new B2UploadManagerItem
            {
                OpenStream = () => new MemoryStream(buffer),
                FileName = $"{mediaGroupData.FluffleId}/index.json",
                ContentType = MimeType.Json
            }, mediaGroupData.Priority);

            _logger.LogDebug("Uploaded index file for media group with ID {id}.", mediaGroupData.FluffleId);
        }

        public async Task NotifyChatAsync(MediaGroupData data)
        {
            data.MongoMediaGroup.HasResults = true;
            await _botContext.MediaGroups.ReplaceAsync(x => x.Id == data.Id, data.MongoMediaGroup);

            var message = data.Items[0].Message;
            var response = new ReverseSearchResponse
            {
                Chat = data.Chat
            };
            if (data.Chat.Type == ChatType.Channel)
            {
                response.IsTextCaption = true;
                response.MessageId = data.CaptionedMessage.MessageId;
                response.ExistingText = data.CaptionedMessage.Caption;
                response.ExistingTextEntities = data.CaptionedMessage.CaptionEntities;
                message.ReverseSearchFormat = ReverseSearchFormat.Text;
            }
            else
            {
                response.ReplyToMessageId = data.Items.OrderBy(x => x.Message.MessageId).First().Message.MessageId;
            }

            var url = new Uri(new Uri(_botConfiguration.FluffleBaseUrl, UriKind.Absolute), data.FluffleId);
            Formatter.RouteMediaGroup(url.AbsoluteUri, message, response);
            await response.Process(_botClient);
        }

        public class IndexItem
        {
            public IndexImage Image { get; set; }

            public IEnumerable<FluffleResult> Results { get; set; }
        }

        public class IndexImage
        {
            public string Id { get; set; }

            public int Width { get; set; }

            public int CenterX { get; set; }

            public int Height { get; set; }

            public int CenterY { get; set; }
        }

        public async Task HandleAsync(MediaGroupData data)
        {
            try
            {
                _logger.LogDebug("Waiting for all media group images to finish reverse searching.");

                async Task<MediaGroupItem> WaitForReverseSearch(MediaGroupItem item)
                {
                    try
                    {
                        await item.ReverseSearchEvent.WaitAsync(new CancellationTokenSource(ReverseSearchTimeout).Token);

                        return item;
                    }
                    catch (TaskCanceledException)
                    {
                    }

                    return null;
                }

                var reverseSearchTasks = data.Items
                    .Select(x => Task.Run(() => WaitForReverseSearch(x)))
                    .ToHashSet();

                var completedItems = new List<MediaGroupItem>();
                var foundAny = false;
                while (reverseSearchTasks.Any())
                {
                    var completed = await Task.WhenAny(reverseSearchTasks);
                    var result = await completed;

                    if (result != null)
                    {
                        if (!foundAny && result.Message.FluffleResponse.Results.Any())
                        {
                            data.NotifyChatTask = Task.Run(() => NotifyChatAsync(data));
                            foundAny = true;
                        }

                        completedItems.Add(result);
                    }

                    reverseSearchTasks.Remove(completed);
                }

                if (!foundAny)
                    return;

                async Task GenerateAndUploadThumbnail(MediaGroupItem item)
                {
                    _logger.LogDebug("Processing image from message with ID {messageId}.", item.Message.MessageId);

                    try
                    {
                        // Flush image data to temporary file
                        using var inputFile = new TemporaryFile();
                        await using (var inputFileStream = inputFile.OpenFileStream())
                        {
                            await using var imageStream = new MemoryStream(item.Image);
                            await imageStream.CopyToAsync(inputFileStream);
                        }

                        // Generate the thumbnail and flush it to another temporary file
                        using var outputFile = new TemporaryFile();
                        var result = _fluffleThumbnail.Generate(inputFile.Location, outputFile.Location, ThumbnailTargetSize, ImageFormatConstant.Jpeg, ThumbnailQuality);
                        item.Thumbnail = new MediaGroupItemThumbnail
                        {
                            Result = result,
                            Data = await File.ReadAllBytesAsync(outputFile.Location)
                        };

                        // Upload the thumbnail to Backblaze B2
                        await _uploadManagerCollection.Thumbnail.ProcessAsync(new B2UploadManagerItem
                        {
                            OpenStream = () => new MemoryStream(item.Thumbnail.Data),
                            FileName = $"{data.FluffleId}/{item.Id}.jpg",
                            ContentType = MimeType.Jpeg
                        }, item.Priority);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "Something went wrong while trying to generate a thumbnail.");
                    }
                }

                var thumbnailingTasks = completedItems.Select(x => Task.Run(() => GenerateAndUploadThumbnail(x))).ToArray();
                await Task.WhenAll(thumbnailingTasks);

                var index = completedItems
                    .OrderBy(x => x.Message.MessageId)
                    .Select(item => new IndexItem
                    {
                        Image = item.Thumbnail == null
                            ? null
                            : new IndexImage
                            {
                                Id = item.Id,
                                Width = item.Thumbnail.Result.Width,
                                CenterX = item.Thumbnail.Result.CenterX,
                                Height = item.Thumbnail.Result.Height,
                                CenterY = item.Thumbnail.Result.CenterY
                            },
                        Results = item.Message.FluffleResponse.Results
                    }).ToList();

                await data.NotifyChatTask;
                await UploadIndex(data, index);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Something went wrong while trying to process a media group.");
                throw;
            }
            finally
            {
                data.ProcessedEvent.Set();
            }
        }
    }
}
