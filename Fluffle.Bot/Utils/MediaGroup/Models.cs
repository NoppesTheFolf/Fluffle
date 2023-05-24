using Nito.AsyncEx;
using Noppes.Fluffle.Bot.Database;
using Noppes.Fluffle.Thumbnail;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Noppes.Fluffle.Bot.Utils;

public class MediaGroupData
{
    public string Id { get; set; }

    public int Priority { get; set; }

    public Chat Chat { get; set; }

    public string FluffleId { get; set; }

    public MongoMediaGroup MongoMediaGroup { get; set; }

    public Task NotifyChatTask { get; set; }

    public List<MediaGroupItem> Items { get; set; }

    public MongoMessage CaptionedMessage { get; set; }

    public bool ShouldControllerContinue { get; set; }

    public AsyncManualResetEvent AllReceivedEvent { get; set; }

    public AsyncManualResetEvent ProcessedEvent { get; set; }

    public Func<MediaGroupData, Task> WhenAllReceived { get; set; }

    public DateTime LastUpdateReceivedAt { get; set; }
}

public class MediaGroupItem
{
    public string Id { get; set; }

    public AsyncManualResetEvent ReverseSearchEvent { get; set; }

    public byte[] Image { get; set; }

    public MediaGroupItemThumbnail Thumbnail { get; set; }

    public MongoMessage Message { get; set; }

    public int Priority { get; set; }
}

public class MediaGroupItemThumbnail
{
    public FluffleThumbnailResult Result { get; set; }

    public byte[] Data { get; set; }
}
