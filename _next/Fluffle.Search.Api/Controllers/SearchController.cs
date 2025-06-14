using Fluffle.Imaging.Api.Client;
using Fluffle.Imaging.Api.Models;
using Fluffle.Inference.Api.Client;
using Fluffle.Search.Api.Models;
using Fluffle.Search.Api.Validation;
using Fluffle.Vector.Api.Client;
using Fluffle.Vector.Api.Models.Vectors;
using Microsoft.AspNetCore.Mvc;

namespace Fluffle.Search.Api.Controllers;

[ApiController]
public class SearchController : ControllerBase
{
    private readonly IImagingApiClient _imagingApiClient;
    private readonly IInferenceApiClient _inferenceApiClient;
    private readonly IVectorApiClient _vectorApiClient;

    public SearchController(IImagingApiClient imagingApiClient, IInferenceApiClient inferenceApiClient, IVectorApiClient vectorApiClient)
    {
        _imagingApiClient = imagingApiClient;
        _inferenceApiClient = inferenceApiClient;
        _vectorApiClient = vectorApiClient;
    }

    [HttpPost("/exact-search", Name = "ExactSearch")]
    public async Task<IActionResult> Get(SearchModel model)
    {
        var validationResult = await new SearchModelValidator().ValidateAsync(model);
        if (!validationResult.IsValid)
        {
            return Error.Create(400, validationResult);
        }

        await using var fileStream = model.File.OpenReadStream();

        byte[] thumbnail;
        try
        {
            (thumbnail, _) = await _imagingApiClient.CreateThumbnailAsync(fileStream, size: 300, quality: 95, calculateCenter: false);
        }
        catch (ImagingApiException e)
        {
            if (e.Code == ImagingErrorCode.UnsupportedImage)
            {
                return Error.Create(
                    statusCode: 415,
                    code: "unsupportedFileType",
                    message: "The type of the submitted file isn't supported. Only JPEG, PNG, WebP and GIF are. " +
                             "If you're getting this error even though the image seems te be valid, it might be that the extension is not representative of the encoding."
                );
            }

            if (e.Code == ImagingErrorCode.ImageAreaTooLarge)
            {
                return Error.Create(
                    statusCode: 400,
                    code: "areaTooLarge",
                    message: "The submitted image has an area (width * height) greater than the maximum allowed area of 16 megapixels."
                );
            }

            return Error.Create(
                statusCode: 422,
                code: "corruptFile",
                message: "The submitted file could not be read by Fluffle. This likely means it's corrupt."
            );
        }

        using var thumbnailStream = new MemoryStream(thumbnail);
        var vectors = await _inferenceApiClient.CreateAsync([thumbnailStream]);
        var vector = vectors[0];

        var vectorSearchResults = await _vectorApiClient.SearchCollectionAsync("exactMatchV1", new VectorSearchParametersModel
        {
            Query = vector,
            Limit = model.Limit
        });
        var vectorSearchResultsLookup = vectorSearchResults.ToDictionary(x => x.ItemId);

        var items = await _vectorApiClient.GetItemsAsync(vectorSearchResultsLookup.Keys);

        var models = items
            .Select(x =>
            {
                var vectorSearchResult = vectorSearchResultsLookup[x.ItemId];

                var authors = (x.Properties["authors"]?.AsArray() ?? [])
                    .Select(authorNode => new SearchResultAuthorModel
                    {
                        Id = authorNode!["id"]!.GetValue<string>(),
                        Name = authorNode["name"]!.GetValue<string>()
                    })
                    .ToList();

                return new SearchResultModel
                {
                    Id = x.ItemId,
                    Score = vectorSearchResult.Distance,
                    Platform = x.ItemId.Split('_', 2)[0],
                    Url = x.Properties["url"]!.GetValue<string>(),
                    IsSfw = x.Properties["isSfw"]!.GetValue<bool>(),
                    Thumbnail = x.Thumbnail == null
                        ? null
                        : new SearchResultThumbnailModel
                        {
                            Width = x.Thumbnail.Width,
                            Height = x.Thumbnail.Height,
                            CenterX = x.Thumbnail.CenterX,
                            CenterY = x.Thumbnail.CenterY,
                            Url = x.Thumbnail.Url
                        },
                    Authors = authors
                };
            })
            .OrderByDescending(x => x.Score)
            .ToList();

        return Ok(new SearchResultsModel
        {
            Id = Guid.NewGuid().ToString(),
            Results = models
        });
    }
}
