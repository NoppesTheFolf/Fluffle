using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Nitranium.PerceptualHashing.Utils;
using Noppes.Fluffle.Constants;
using Noppes.Fluffle.Search.Api.Filters;
using Noppes.Fluffle.Search.Api.LinkCreation;
using Noppes.Fluffle.Search.Api.Models;
using Noppes.Fluffle.Search.Api.Services;
using Noppes.Fluffle.Search.Database;
using Noppes.Fluffle.Search.Database.Models;
using Noppes.Fluffle.Thumbnail;
using Noppes.Fluffle.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Search.Api.Controllers;

[TypeFilter(typeof(RequireUserAgentFilter))]
[TypeFilter(typeof(SimilarityServiceReadyFilter))]
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
            {new GifSignature(), FileFormatConstant.Gif},
            {new WebPSignature(), FileFormatConstant.WebP}
        };

        SafeFileSignatures = new FileSignatureMatcher(FileSignatureLookup.Keys.ToArray());
    }

    private readonly ISearchService _searchService;
    private readonly FluffleThumbnail _thumbnail;
    private readonly LinkCreatorStorage _linkCreatorStorage;
    private readonly LinkCreatorRetriever _linkCreatorRetriever;
    private readonly FluffleSearchContext _context;
    private readonly ILogger<SearchApiController> _logger;

    public SearchApiController(ISearchService searchService, FluffleThumbnail thumbnail, LinkCreatorStorage linkCreatorStorage,
        LinkCreatorRetriever linkCreatorRetriever, FluffleSearchContext context, ILogger<SearchApiController> logger)
    {
        _searchService = searchService;
        _thumbnail = thumbnail;
        _linkCreatorStorage = linkCreatorStorage;
        _linkCreatorRetriever = linkCreatorRetriever;
        _context = context;
        _logger = logger;
    }

    [AllowAnonymous]
    [HttpPost("search")]
    public async Task<IActionResult> Search([FromForm] SearchModel model)
    {
        if (model.File.Length > SearchModelValidator.SizeMax)
            return HandleV1(SearchError.FileTooLarge(model.File.Length));

        var request = new SearchRequestV2
        {
            Id = $"{ShortUuidDateTime.ToString(DateTime.UtcNow)}{ShortUuid.Random(12)}",
            Version = Project.Version,
            From = HttpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = Request.Headers["User-Agent"]
        };

        if (request.From == null)
            _logger.LogWarning("Request is missing remote IP address. Has the server been configured correctly?");

        if (model.CreateLink)
            request.LinkCreated = false;

        try
        {
            var stopwatch = CheckpointStopwatch.StartNew(request);

            // Write the content embedded in the request to a temporary file
            using var stopwatchScope = stopwatch.ForCheckpoint(t => t.Flush);
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

            // Check if the submitted image its area is within what Fluffle considers reasonable
            stopwatchScope.Next(t => t.AreaCheck);
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

            var result = await _searchService.SearchAsync(temporaryFile.Location, model.IncludeNsfw, model.Limit, model.Platforms, IsDebug, stopwatchScope);

            stopwatchScope.Next(t => t.LinkCreationPreparation);
            await result.HandleAsync(_ => Task.FromResult(string.Empty), async response =>
            {
                response.Id = request.Id;

                if (!model.CreateLink)
                    return string.Empty;

                await _linkCreatorStorage.SaveAsync(request.Id, temporaryFile.Location, response);

                return string.Empty;
            });

            if (model.CreateLink)
            {
                stopwatchScope.Next(t => t.EnqueueLinkCreation);
                await _linkCreatorRetriever.Enqueue(request);
            }

            stopwatchScope.Next(t => t.Finish);
            return HandleV1(result, response =>
            {
                request.Count = response.Stats.Count;

                var resultMatchCounts = Enum.GetValues<ResultMatch>().ToDictionary(rm => rm, _ => 0);
                response.Results.GroupBy(r => r.Match).ToList().ForEach(g => resultMatchCounts[g.Key] = g.Count());
                foreach (var (key, count) in resultMatchCounts)
                {
                    switch (key)
                    {
                        case ResultMatch.Exact:
                            request.ExactCount = count;
                            break;
                        case ResultMatch.TossUp:
                            request.TossUpCount = count;
                            break;
                        case ResultMatch.Alternative:
                            request.AlternativeCount = count;
                            break;
                        case ResultMatch.Unlikely:
                            request.UnlikelyCount = count;
                            break;
                    }
                }

                response.Stats.ElapsedMilliseconds = (int)stopwatch.ElapsedMilliseconds;
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
            _context.SearchRequestsV2.Add(request);
            await _context.SaveChangesAsync();
        }
    }
}