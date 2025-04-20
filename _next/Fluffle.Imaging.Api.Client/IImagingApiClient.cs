namespace Fluffle.Imaging.Api.Client;

public interface IImagingApiClient
{
    Task<ThumbnailModel> CreateThumbnailAsync(Stream imageStream, int size, int quality);

    Task<ImageMetadataModel> GetMetadataAsync(Stream imageStream);
}
