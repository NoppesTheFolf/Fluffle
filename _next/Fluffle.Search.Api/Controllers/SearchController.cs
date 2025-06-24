using Fluffle.Imaging.Api.Client;
using Fluffle.Imaging.Api.Models;
using Fluffle.Inference.Api.Client;
using Fluffle.Search.Api.Models;
using Fluffle.Search.Api.Validation;
using Fluffle.Vector.Api.Client;
using Fluffle.Vector.Api.Models.Vectors;
using Fluffle_Search_Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.ML;

namespace Fluffle.Search.Api.Controllers;

[ApiController]
public class SearchController : ControllerBase
{
    private readonly IImagingApiClient _imagingApiClient;
    private readonly IInferenceApiClient _inferenceApiClient;
    private readonly IVectorApiClient _vectorApiClient;
    private readonly PredictionEnginePool<ExactMatchV2IsMatch.ModelInput, ExactMatchV2IsMatch.ModelOutput> _isMatchModel;

    public SearchController(
        IImagingApiClient imagingApiClient,
        IInferenceApiClient inferenceApiClient,
        IVectorApiClient vectorApiClient,
        PredictionEnginePool<ExactMatchV2IsMatch.ModelInput, ExactMatchV2IsMatch.ModelOutput> isMatchModel)
    {
        _imagingApiClient = imagingApiClient;
        _inferenceApiClient = inferenceApiClient;
        _vectorApiClient = vectorApiClient;
        _isMatchModel = isMatchModel;
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
        var vectors = await _inferenceApiClient.ExactMatchV2Async([thumbnailStream]);
        var vector = vectors[0];

        var vectorSearchResults = await _vectorApiClient.SearchCollectionAsync("exactMatchV2", new VectorSearchParametersModel
        {
            Query = vector,
            Limit = model.Limit + 4 // An extra 4 so that D2-D5 are filled with real values for the last item
        });
        var vectorSearchResultsLookup = vectorSearchResults.ToDictionary(x => x.ItemId);

        var isMatchPredictions = new Dictionary<string, bool>();
        for (var i = 0; i < vectorSearchResults.Count; i++)
        {
            var modelInput = new ExactMatchV2IsMatch.ModelInput
            {
                D1 = vectorSearchResults[i].Distance,
                D2 = i + 1 < vectorSearchResults.Count ? vectorSearchResults[i + 1].Distance : 0,
                D3 = i + 2 < vectorSearchResults.Count ? vectorSearchResults[i + 2].Distance : 0,
                D4 = i + 3 < vectorSearchResults.Count ? vectorSearchResults[i + 3].Distance : 0,
                D5 = i + 4 < vectorSearchResults.Count ? vectorSearchResults[i + 4].Distance : 0,
            };
            var modelOutput = _isMatchModel.Predict(modelInput);

            isMatchPredictions[vectorSearchResults[i].ItemId] = modelOutput.PredictedLabel;
        }

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
                    Distance = vectorSearchResult.Distance,
                    Match = isMatchPredictions[x.ItemId] ? SearchResultModelMatch.Exact : SearchResultModelMatch.Unlikely,
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
            .OrderByDescending(x => x.Distance)
            .Take(model.Limit)
            .ToList();

        var probableModels = models
            .Where(x => x.Match == SearchResultModelMatch.Exact)
            .GroupBy(x => x.Platform)
            .SelectMany(x => x.OrderByDescending(x => x.Distance).Skip(1).ToList());

        foreach (var probableModel in probableModels)
        {
            probableModel.Match = SearchResultModelMatch.Probable;
        }

        return Ok(new SearchResultsModel
        {
            Id = Guid.NewGuid().ToString(),
            Results = models
        });
    }
}
