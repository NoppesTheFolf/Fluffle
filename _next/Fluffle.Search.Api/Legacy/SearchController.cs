using Fluffle.Imaging.Api.Models;
using Fluffle.Search.Api.IdGeneration;
using Fluffle.Search.Api.Legacy;
using Fluffle.Search.Api.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.IO.Hashing;
using System.Text;

// ReSharper disable once CheckNamespace
namespace Fluffle.Search.Api.Controllers;

public partial class SearchController
{
    [HttpPost("/v1/search", Name = "LegacySearch")]
    public async Task<IActionResult> LegacySearchAsync([FromForm] LegacySearchModel model)
    {
        var stopwatch = Stopwatch.StartNew();

        var validationResult = await new LegacySearchModelValidator().ValidateAsync(model);
        if (!validationResult.IsValid)
        {
            var error = new LegacyValidationError
            {
                Code = "VALIDATION_FAILED",
                Message = "One or more validation errors occurred.",
                Errors = validationResult.Errors
                    .GroupBy(x => x.PropertyName)
                    .ToDictionary(x => x.Key.Camelize(), x => x.Select(y => y.ErrorMessage))
            };

            return BadRequest(error);
        }

        await using var fileStream = model.File!.OpenReadStream();

        var (thumbnail, thumbnailError) = await CreateThumbnailAsync(fileStream);
        if (thumbnail == null)
        {
            if (thumbnailError == ImagingErrorCode.ImageAreaTooLarge)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new LegacyError
                {
                    Code = "AREA_TOO_LARGE",
                    Message = "The submitted image has an area (width * height) greater than the maximum allowed area of 16 megapixels."
                });
            }

            if (thumbnailError == ImagingErrorCode.UnsupportedImage)
            {
                return StatusCode(StatusCodes.Status415UnsupportedMediaType, new LegacyError
                {
                    Code = "UNSUPPORTED_FILE_TYPE",
                    Message = "The type of the submitted file isn't supported. Only JPEG, PNG, WebP and GIF are."
                });
            }

            return StatusCode(StatusCodes.Status422UnprocessableEntity, new LegacyError
            {
                Code = "CORRUPT_FILE",
                Message = "The submitted file couldn't be read by Fluffle. This likely means it's corrupt."
            });
        }

        using var thumbnailStream = new MemoryStream(thumbnail);
        var searchModels = await ExactSearchAsync(thumbnailStream, Math.Max(model.Limit * 4, 128));

        if (!model.IncludeNsfw)
        {
            searchModels = searchModels.Where(x => x.IsSfw).ToList();
        }

        if (model.Platforms != null && model.Platforms.Count > 0)
        {
            var platforms = model.Platforms.ToHashSet(StringComparer.OrdinalIgnoreCase);
            searchModels = searchModels.Where(x => platforms.Contains(x.Platform)).ToList();
        }

        searchModels = searchModels.Take(model.Limit).ToList();

        var requestId = $"{ShortUuidDateTime.ToString(DateTime.UtcNow)}{ShortUuid.Random(12)}";
        return Ok(new LegacySearchResultsModel
        {
            Id = requestId,
            Stats = new LegacySearchResultStatsModel
            {
                Count = 82_000_000,
                ElapsedMilliseconds = (int)stopwatch.ElapsedMilliseconds
            },
            Results = searchModels.Select(x =>
            {
                var match = x.Match switch
                {
                    SearchResultModelMatch.Exact => LegacySearchResultMatchModel.Exact,
                    SearchResultModelMatch.Probable => LegacySearchResultMatchModel.TossUp,
                    _ => LegacySearchResultMatchModel.Unlikely
                };

                return new LegacySearchResultModel
                {
                    Id = Math.Abs((int)XxHash32.HashToUInt32(Encoding.UTF8.GetBytes(x.Id))),
                    Score = x.Distance,
                    Match = match,
                    Platform = x.Platform,
                    Location = x.Url,
                    IsSfw = x.IsSfw,
                    Thumbnail = x.Thumbnail == null ? null : new LegacySearchResultThumbnailModel
                    {
                        Width = x.Thumbnail.Width,
                        CenterX = x.Thumbnail.CenterX,
                        Height = x.Thumbnail.Height,
                        CenterY = x.Thumbnail.CenterY,
                        Location = x.Thumbnail.Url
                    },
                    Credits = x.Authors.Select(y => new LegacySearchResultCreditModel
                    {
                        Id = Math.Abs((int)XxHash32.HashToUInt32(Encoding.UTF8.GetBytes(y.Id))),
                        Name = y.Name
                    }).ToList()
                };
            }).ToList()
        });
    }
}
