using Noppes.Fluffle.Api;
using Noppes.Fluffle.Configuration;
using Noppes.Fluffle.Constants;
using Noppes.Fluffle.Search.Api.Models;
using Noppes.Fluffle.Thumbnail;
using System.IO;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Search.Api.LinkCreation
{
    public class LinkCreatorStorage
    {
        private const int ThumbnailTarget = 350;
        private const int ThumbnailQuality = 85;

        private readonly SearchServerConfiguration _configuration;
        private readonly FluffleThumbnail _thumbnail;

        public LinkCreatorStorage(SearchServerConfiguration configuration, FluffleThumbnail thumbnail)
        {
            _configuration = configuration;
            _thumbnail = thumbnail;
        }

        public async Task SaveAsync(string id, string imageLocation, SearchResultModel searchResult)
        {
            // Generate JPEG thumbnail
            _thumbnail.Generate(imageLocation, GetThumbnailLocation(id), ThumbnailTarget, ImageFormatConstant.Jpeg, ThumbnailQuality);

            // Save search results as JSON
            var searchResultJson = AspNetJsonSerializer.Serialize(searchResult);
            await File.WriteAllTextAsync(GetSearchResultsLocation(id), searchResultJson);
        }

        public string GetThumbnailLocation(string id) => Path.Join(_configuration.SearchResultsTemporaryLocation, $"{id}.jpg");
        public string GetSearchResultsLocation(string id) => Path.Join(_configuration.SearchResultsTemporaryLocation, $"{id}.json");
    }
}
