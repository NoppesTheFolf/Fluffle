namespace Fluffle.Ingestion.Worker.ThumbnailStorage;

public interface IThumbnailStorage
{
    Task<string> PutAsync(string itemId, Stream thumbnailStream);

    Task DeleteAsync(string itemId);
}
