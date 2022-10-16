using Noppes.Fluffle.Api.Services;
using Noppes.Fluffle.Constants;
using Noppes.Fluffle.Search.Api.Models;
using Noppes.Fluffle.Search.Database.Models;
using Noppes.Fluffle.Utils;
using System.Collections.Immutable;
using System.Net;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Search.Api.Services
{
    public static class SearchError
    {
        private const string UnsupportedFileTypeCode = "UNSUPPORTED_FILE_TYPE";

        private const string CorruptImageCode = "CORRUPT_FILE";

        private const string FileTooLargeCode = "FILE_TOO_LARGE";

        private const string AreaTooLargeCode = "AREA_TOO_LARGE";

        public static SE UnsupportedFileType() => new(UnsupportedFileTypeCode, HttpStatusCode.UnsupportedMediaType,
            "The type of the submitted file isn't supported. Only JPEG, PNG, WebP and GIF are. " +
            "If you're getting this error even though the image seems te be valid, check if the image is properly encoded.");

        public static SE CorruptImage() => new(CorruptImageCode, HttpStatusCode.UnprocessableEntity,
            "The submitted file couldn't be read by Fluffle. This likely means it's corrupt.");

        public static SE FileTooLarge(long length) => new(FileTooLargeCode, HttpStatusCode.RequestEntityTooLarge,
            $"The submitted file has a size of {length} bytes while the maximum allowed size is {SearchModelValidator.SizeMax} bytes (4 MiB).");

        public static SE AreaTooLarge(int area) => new(AreaTooLargeCode, HttpStatusCode.BadRequest,
            $"The submitted image has an area (width * height) of {area} pixels while the maximum allowed area is {SearchModelValidator.AreaMax} pixels.");
    }

    public readonly struct HashCollection
    {
        public ulong[] Red { get; }

        public ulong[] Green { get; }

        public ulong[] Blue { get; }

        public ulong[] Average { get; }

        public HashCollection(ulong[] red, ulong[] green, ulong[] blue, ulong[] average)
        {
            Red = red;
            Green = green;
            Blue = blue;
            Average = average;
        }
    }

    public interface ISearchService
    {
        public Task<SR<SearchResultModel>> SearchAsync(string imageLocation, bool includeNsfw, int limit, ImmutableHashSet<PlatformConstant> platforms, bool includeDebug, CheckpointStopwatchScope<SearchRequestV2> scope);

        public Task<SR<SearchResultModel>> SearchAsync(ImageHash hash, bool includeNsfw, int limit, ImmutableHashSet<PlatformConstant> platforms, bool includeDebug, CheckpointStopwatchScope<SearchRequestV2> scope);

        public Task<SR<SearchResultModel>> SearchAsync(ulong hash64, HashCollection hashes256, HashCollection hashes1024, bool includeNsfw, int limit, ImmutableHashSet<PlatformConstant> platforms, bool includeDebug, CheckpointStopwatchScope<SearchRequestV2> scope);
    }
}
