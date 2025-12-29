using Fluffle.Content.Api.Client;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace Fluffle.Ingestion.Worker.ThumbnailStorage;

public class ContentApiThumbnailStorage : IThumbnailStorage
{
    private readonly IContentApiClient _contentApiClient;
    private readonly IOptions<ContentApiClientOptions> _contentApiClientOptions;
    private readonly IOptions<ThumbnailStorageOptions> _thumbnailOptions;

    public ContentApiThumbnailStorage(
        IContentApiClient contentApiClient,
        IOptions<ContentApiClientOptions> contentApiClientOptions,
        IOptions<ThumbnailStorageOptions> thumbnailOptions)
    {
        _contentApiClient = contentApiClient;
        _contentApiClientOptions = contentApiClientOptions;
        _thumbnailOptions = thumbnailOptions;
    }

    public async Task<string> PutAsync(string itemId, Stream thumbnailStream)
    {
        var path = GetPath(itemId);
        await _contentApiClient.PutAsync(path, thumbnailStream);

        var url = new Uri(new Uri(_contentApiClientOptions.Value.Url), path).AbsoluteUri;
        return url;
    }

    public async Task DeleteAsync(string itemId)
    {
        var path = GetPath(itemId);
        await _contentApiClient.DeleteAsync(path);
    }

    private string GetPath(string itemId)
    {
        var saltedItemId = $"{_thumbnailOptions.Value.Salt}:{itemId}";
        var saltedItemIdSha1Bytes = SHA1.HashData(Encoding.UTF8.GetBytes(saltedItemId));
        var itemIdHash = Convert.ToHexStringLower(saltedItemIdSha1Bytes);
        var path = $"thumbnails/{itemIdHash[..2]}/{itemIdHash[2..4]}/{itemIdHash}.jpg";

        return path;
    }
}
