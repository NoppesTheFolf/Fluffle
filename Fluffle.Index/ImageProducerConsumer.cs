using Nitranium.PerceptualHashing.Utils;
using Noppes.Fluffle.Constants;
using Noppes.Fluffle.Http;
using Noppes.Fluffle.Main.Client;
using Noppes.Fluffle.Main.Communication;
using Noppes.Fluffle.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Index;

public static class ImageProducerConsumerHelper
{
    public static async Task<bool> OnProducedAsync(FluffleClient fluffleClient, ChannelImage image)
    {
        if (image.Warnings.Any())
        {
            foreach (var warning in image.Warnings)
            {
                await HttpResiliency.RunAsync(() => fluffleClient.PutContentWarningAsync(image.Content.PlatformName, image.Content.IdOnPlatform, new PutWarningModel
                {
                    Warning = warning
                }));
            }

            image.Warnings.Clear();
        }

        if (image.Error != null)
        {
            await HttpResiliency.RunAsync(() => fluffleClient.PutContentErrorAsync(image.Content.PlatformName, image.Content.IdOnPlatform, new PutErrorModel
            {
                Error = image.Error,
                IsFatal = true
            }));

            return false;
        }

        return true;
    }
}

public abstract class ImageProducer : Producer<ChannelImage>
{
    protected readonly FluffleClient FluffleClient;

    protected ImageProducer(FluffleClient fluffleClient)
    {
        FluffleClient = fluffleClient;
    }

    public override Task<bool> OnProducedAsync(ChannelImage image) =>
        ImageProducerConsumerHelper.OnProducedAsync(FluffleClient, image);
}

public abstract class ImageConsumer : Consumer<ChannelImage>
{
    protected readonly FluffleClient FluffleClient;

    public override Task<bool> OnProducedAsync(ChannelImage image) =>
        ImageProducerConsumerHelper.OnProducedAsync(FluffleClient, image);

    protected ImageConsumer(FluffleClient fluffleClient)
    {
        FluffleClient = fluffleClient;
    }
}

public sealed class ChannelImage : IDisposable
{
    public UnprocessedContentModel Content { get; set; }

    public TemporaryFile File { get; set; }

    public PutImageIndexModel.ImageHashesModel Hashes { get; set; }

    public Thumbnail Thumbnail { get; set; }

    public string Error { get; set; }

    public ICollection<string> Warnings { get; set; }

    public ChannelImage()
    {
        Warnings = new List<string>();
    }

    public void Dispose()
    {
        File?.Dispose();
        Thumbnail?.File?.Dispose();
    }
}

public class Thumbnail
{
    public ThumbnailTemplate Template { get; set; }

    public int Width { get; set; }

    public int CenterX { get; set; }

    public int Height { get; set; }

    public int CenterY { get; set; }

    public ImageFormatConstant Format { get; set; }

    public string Location { get; set; }

    public string Filename { get; set; }

    public string B2FileId { get; set; }

    public TemporaryFile File { get; set; }
}
