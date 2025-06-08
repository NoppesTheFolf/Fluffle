using Fluffle.Imaging.Api.Models;

namespace Fluffle.Imaging.Api.Client;

public interface IImagingApiClient
{
    Task<ImageMetadataModel> GetMetadataAsync(Stream imageStream);

    Task<(byte[] thumbnail, ImageMetadataModel metadata)> CreateThumbnailAsync(Stream imageStream, int size, int quality, bool calculateCenter);
}
