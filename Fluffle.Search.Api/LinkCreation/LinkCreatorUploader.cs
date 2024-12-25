using Noppes.Fluffle.B2;
using Noppes.Fluffle.Search.Database.Models;
using Noppes.Fluffle.Utils;
using System.IO;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Search.Api.LinkCreation;

public class LinkCreatorUploader : Consumer<SearchRequest>
{
    private readonly LinkCreatorStorage _storage;
    private readonly B2Bucket _bucket;

    public LinkCreatorUploader(LinkCreatorStorage storage, B2ClientCollection b2ClientCollection)
    {
        _storage = storage;
        _bucket = b2ClientCollection.SearchResultsClient;
    }

    public override async Task<SearchRequest> ConsumeAsync(SearchRequest data)
    {
        var thumbnailLocation = _storage.GetThumbnailLocation(data.Id);
        await UploadAsync(data.Id, thumbnailLocation, "image/jpeg");

        var searchResultsLocation = _storage.GetSearchResultsLocation(data.Id);
        await UploadAsync(data.Id, searchResultsLocation, "application/json");

        return data;
    }

    private async Task UploadAsync(string id, string location, string contentType)
    {
        if (!File.Exists(location))
            return;

        var extension = Path.GetExtension(location);
        await _bucket.UploadAsync(() => File.OpenRead(location), $"{id}{extension}", contentType);

        File.Delete(location);
    }
}
