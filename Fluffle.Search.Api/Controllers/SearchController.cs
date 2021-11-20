using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nitranium.PerceptualHashing.Utils;
using Noppes.Fluffle.Constants;
using Noppes.Fluffle.Search.Api.Filters;
using Noppes.Fluffle.Search.Api.Models;
using Noppes.Fluffle.Search.Api.Services;
using Noppes.Fluffle.Search.Database.Models;
using Noppes.Fluffle.Thumbnail;
using Noppes.Fluffle.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Search.Api.Controllers
{
    [TypeFilter(typeof(RequireUserAgentFilter))]
    [TypeFilter(typeof(StartupFilter))]
    public class SearchApiController : SearchApiControllerV1
    {
        private static readonly IReadOnlyDictionary<FileSignature, FileFormatConstant> FileSignatureLookup;
        private static readonly FileSignatureMatcher SafeFileSignatures;

        static SearchApiController()
        {
            FileSignatureLookup = new Dictionary<FileSignature, FileFormatConstant>
            {
                {new JpegSignature(), FileFormatConstant.Jpeg},
                {new PngSignature(), FileFormatConstant.Png},
                {new WebPSignature(), FileFormatConstant.WebP}
            };

            SafeFileSignatures = new FileSignatureMatcher(FileSignatureLookup.Keys.ToArray());
        }

        private readonly ISearchService _searchService;
        private readonly FluffleThumbnail _thumbnail;
        private readonly FluffleSearchContext _context;
        private readonly ApiBehaviorOptions _options;
        private readonly ILogger<SearchApiController> _logger;

        public SearchApiController(ISearchService searchService, FluffleThumbnail thumbnail, FluffleSearchContext context,
            IOptions<ApiBehaviorOptions> options, ILogger<SearchApiController> logger)
        {
            _searchService = searchService;
            _thumbnail = thumbnail;
            _context = context;
            _options = options.Value;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpPost("search")]
        public async Task<IActionResult> Search([FromForm] SearchModel model)
        {
            if (model.File != null && model.File.Length > SearchModelValidator.SizeMax)
                return HandleV1(SearchError.FileTooLarge(model.File.Length));

            var request = new SearchRequest
            {
                From = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = Request.Headers["User-Agent"]
            };
            var stopwatch = CheckpointStopwatch.StartNew(request);

            if (request.From == null)
                _logger.LogWarning("Request is missing remote IP address. Has the server been configured correctly?");

            try
            {
                using var scope = stopwatch.ForCheckpoint(t => t.Flush);

                // Write the content embedded in the request to a temporary file
                using var temporaryFile = new TemporaryFile();
                await using (var temporaryFileStream = temporaryFile.OpenFileStream())
                {
                    await model.File.CopyToAsync(temporaryFileStream);
                    temporaryFileStream.Position = 0;

                    // Check if the uploaded file contains a signature of a web safe image type
                    if (!SafeFileSignatures.TryMatch(temporaryFileStream, out var formatSignature))
                        return HandleV1(SearchError.UnsupportedFileType());

                    request.Format = FileSignatureLookup[formatSignature];
                }

                scope.Next(t => t.AreaCheck);
                try
                {
                    var dimensions = _thumbnail.GetDimensions(temporaryFile.Location);
                    request.Width = dimensions.Width;
                    request.Height = dimensions.Height;

                    if (dimensions.Area > SearchModelValidator.AreaMax)
                        return HandleV1(SearchError.AreaTooLarge(dimensions.Area));
                }
                catch (Exception e)
                {
                    request.Exception = e.ToString();
                    return HandleV1(SearchError.CorruptImage());
                }

                var result = await _searchService.SearchAsync(temporaryFile.Location, model.IncludeNsfw, model.Limit, model.Platforms, IsDebug, scope);
                StartupFilter.HasStarted = true;

                return HandleV1(result, response =>
                {
                    request.Count = response.Stats.Count;

                    return Ok(response);
                });
            }
            catch (Exception e)
            {
                request.Exception = e.ToString();
                throw;
            }
            finally
            {
                _context.SearchRequests.Add(request);
                await _context.SaveChangesAsync();
            }
        }
    }
}
