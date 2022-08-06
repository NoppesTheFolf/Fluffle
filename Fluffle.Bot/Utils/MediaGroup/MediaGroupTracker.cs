using Humanizer;
using Microsoft.Extensions.Logging;
using Nito.AsyncEx;
using Noppes.Fluffle.Bot.Database;
using Noppes.Fluffle.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Noppes.Fluffle.Bot.Utils
{
    public class MediaGroupTracker
    {
        private static readonly TimeSpan CheckInterval = 500.Milliseconds();
        private static readonly TimeSpan ProcessTimeout = 2.Seconds();

        private int _priority;
        private readonly AsyncLock _mutex;
        private readonly IDictionary<string, MediaGroupData> _mediaGroups;
        private readonly MediaGroupHandler _mediaGroupHandler;
        private readonly BotContext _botContext;
        private readonly ILogger<MediaGroupTracker> _logger;

        public MediaGroupTracker(MediaGroupHandler mediaGroupHandler, BotContext botContext, ILogger<MediaGroupTracker> logger)
        {
            _mutex = new AsyncLock();
            _mediaGroups = new Dictionary<string, MediaGroupData>();

            _mediaGroupHandler = mediaGroupHandler;
            _botContext = botContext;
            _logger = logger;

            Task.Run(CheckContinuously);
        }

        public async Task<(MediaGroupData, MediaGroupItem)> TrackAsync(Chat chat, MongoMessage message)
        {
            using var _ = await _mutex.LockAsync();

            var now = DateTime.UtcNow;
            if (!_mediaGroups.TryGetValue(message.MediaGroupId, out var data))
            {
                // Generate a nicely compact ID of 12 characters that is not in use yet. The ID
                // contains the date at which the first message in the media group got tracked.
                string fluffleId;
                do
                {
                    fluffleId = $"{ShortUuidDateTime.ToString(now)}{ShortUuid.Random(7)}";
                } while (await _botContext.MediaGroups.AnyAsync(x => x.FluffleId == fluffleId));

                data = new MediaGroupData
                {
                    Chat = chat,
                    Id = message.MediaGroupId,
                    Priority = _priority++,
                    FluffleId = fluffleId,
                    AllReceivedEvent = new AsyncManualResetEvent(),
                    ProcessedEvent = new AsyncManualResetEvent(),
                    WhenAllReceived = _mediaGroupHandler.HandleAsync,
                    Items = new List<MediaGroupItem>()
                };

                // Store the media group in the database
                data.MongoMediaGroup = new MongoMediaGroup
                {
                    Id = data.Id,
                    FluffleId = data.FluffleId,
                    When = now
                };
                await _botContext.MediaGroups.InsertAsync(data.MongoMediaGroup);

                _mediaGroups.Add(message.MediaGroupId, data);
            }

            var item = new MediaGroupItem
            {
                Id = ShortUuid.Random(12),
                ReverseSearchEvent = new AsyncManualResetEvent(),
                Message = message
            };
            data.Items.Add(item);
            data.LastUpdateReceivedAt = now;

            return (data, item);
        }

        public async Task CheckContinuously()
        {
            // Continuously check if all messages in the media group have been received
            while (true)
            {
                List<MediaGroupData> finishedMediaGroups;
                lock (_mediaGroups)
                {
                    var now = DateTime.UtcNow;
                    finishedMediaGroups = _mediaGroups.Values
                        .Where(mediaGroup => now.Subtract(mediaGroup.LastUpdateReceivedAt) >= ProcessTimeout)
                        .ToList();

                    foreach (var mediaGroup in finishedMediaGroups)
                        _mediaGroups.Remove(mediaGroup.Id);
                }

                foreach (var mediaGroup in finishedMediaGroups)
                {
                    var messagesWithCaption = mediaGroup.Items
                        .Select(x => x.Message)
                        .Where(x => x.Caption != null)
                        .ToList();

                    if (mediaGroup.Chat.Type == ChatType.Channel && messagesWithCaption.Count != 1)
                    {
                        mediaGroup.ShouldControllerContinue = true;
                    }
                    else
                    {
                        mediaGroup.CaptionedMessage = messagesWithCaption.FirstOrDefault();
                        _ = Task.Run(() => mediaGroup.WhenAllReceived(mediaGroup));
                    }

                    mediaGroup.AllReceivedEvent.Set();
                }

                await Task.Delay(CheckInterval);
            }
        }
    }
}
