using Humanizer;
using Noppes.Fluffle.Main.Client;
using Noppes.Fluffle.Main.Communication;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Sync
{
    public static class RetryContentProducer
    {
        public static readonly TimeSpan Interval = 30.Minutes();
        public static readonly TimeSpan Delay = 3.Seconds();
    }

    public class RetryContentProducer<TContentProducer, TContent> : SyncProducer
        where TContentProducer : ContentProducer<TContent>
    {
        private readonly PlatformModel _platform;
        private readonly FluffleClient _fluffleClient;
        private readonly TContentProducer _contentProducer;

        public RetryContentProducer(PlatformModel platform, FluffleClient fluffleClient, TContentProducer contentProducer)
        {
            _platform = platform;
            _fluffleClient = fluffleClient;
            _contentProducer = contentProducer;
        }

        public override async Task WorkAsync()
        {
            var id = await _fluffleClient.GetContentToRetryAsync(_platform.NormalizedName);
            if (id == null)
            {
                Log.Information("No more content to retry. Waiting for {interval}.", RetryContentProducer.Interval);
                await Task.Delay(RetryContentProducer.Interval);
                return;
            }

            Log.Information("Retrying content with ID {id}.", id);
            var src = await _contentProducer.GetContentAsync(id);
            if (src == null)
            {
                Log.Information("Flagging retried content with ID {id} for deletion.", id);
                await _contentProducer.FlagForDeletionAsync(id);
            }
            else
            {
                Log.Information("Submitting retried content with ID {id} for indexing.", id);
                var content = ((IContentMapper<TContent>)_contentProducer).SrcToContent(src);
                await ProduceAsync(new List<PutContentModel> { content });
            }

            await Task.Delay(RetryContentProducer.Delay);
        }
    }
}
