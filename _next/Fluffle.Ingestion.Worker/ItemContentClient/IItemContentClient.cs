using Fluffle.Ingestion.Api.Models.Items;

namespace Fluffle.Ingestion.Worker.ItemContentClient;

public interface IItemContentClient
{
    Task<(ImageModel, Stream)> DownloadAsync(ICollection<ImageModel> images);
}
