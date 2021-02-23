using Noppes.Fluffle.Api.Services;
using Noppes.Fluffle.Search.Api.Models;
using System.Net;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Search.Api.Services
{
    public static class SearchError
    {
        private const string UnsupportedFileTypeCode = "UNSUPPORTED_FILE_TYPE";

        private const string CorruptImageCode = "CORRUPT_IMAGE";

        public static SE UnsupportedFileType() => new(UnsupportedFileTypeCode, HttpStatusCode.UnsupportedMediaType,
            "The type of the submitted file isn't supported. Only JPEG, PNG and WebP are. " +
            "If you're getting this error even though the image seems te be a valid, check if the image is properly encoded.");

        public static SE CorruptImage() => new(CorruptImageCode, HttpStatusCode.UnprocessableEntity,
            "The submitted image couldn't be read by Fluffle. This likely means it's corrupt.");
    }

    public interface ISearchService
    {
        public Task<SR<SearchResultModel>> SearchAsync(SearchModel model);
    }
}
