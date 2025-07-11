using FluentValidation;
using Fluffle.Content.Api.Client;
using Fluffle.Imaging.Api.Client;
using Fluffle.Imaging.Api.Models;
using Fluffle.Inference.Api.Client;
using Fluffle.Search.Api.IdGeneration;
using Fluffle.Search.Api.Models;
using Fluffle.Search.Api.Validation;
using Fluffle.Search.Api.Validation.Validators;
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
    private readonly IContentApiClient _contentApiClient;

    public SearchController(
        IImagingApiClient imagingApiClient,
        IInferenceApiClient inferenceApiClient,
        IVectorApiClient vectorApiClient,
        PredictionEnginePool<ExactMatchV2IsMatch.ModelInput, ExactMatchV2IsMatch.ModelOutput> isMatchModel,
        IContentApiClient contentApiClient)
    {
        _imagingApiClient = imagingApiClient;
        _inferenceApiClient = inferenceApiClient;
        _vectorApiClient = vectorApiClient;
        _isMatchModel = isMatchModel;
        _contentApiClient = contentApiClient;
    }

    [HttpPost("create-link")]
    public async Task<IActionResult> CreateLinkAsync([FromForm] CreateLinkModel model)
    {
        var validationResult = await new CreateLinkModelValidator().ValidateAsync(model);
        if (!validationResult.IsValid)
        {
            return Error.Create(400, validationResult);
        }

        await using var fileStream = model.File.OpenReadStream();
        var (thumbnail, error) = await CreateThumbnailAsync(fileStream);
        if (thumbnail == null)
        {
            return error.AsResult();
        }

        var id = $"{ShortUuidDateTime.ToString(DateTime.UtcNow)}{ShortUuid.Random(12)}";

        using var thumbnailStream = new MemoryStream(thumbnail);
        await _contentApiClient.PutAsync($"users/{id[..2]}/{id[2..4]}/{id}.jpg", thumbnailStream);

        return Ok(new CreateLinkResultModel
        {
            Id = id
        });
    }

    [HttpGet("/exact-search-by-id", Name = "ExactSearchById")]
    public async Task<IActionResult> ExactSearchByIdAsync([FromQuery] SearchByIdModel model)
    {
        var validationResult = await new SearchByIdModelValidator().ValidateAsync(model);
        if (!validationResult.IsValid)
        {
            return Error.Create(400, validationResult);
        }

        await using var stream = await _contentApiClient.GetAsync($"users/{model.Id[..2]}/{model.Id[2..4]}/{model.Id}.jpg");
        if (stream == null)
        {
            return Error.Create(404, null, "No link has been created with the given ID.");
        }

        var searchModels = await ExactSearchAsync(stream, model.Limit);

        return Ok(new SearchResultsModel
        {
            Id = model.Id,
            Results = searchModels
        });
    }

    [HttpPost("/exact-search-by-file", Name = "ExactSearchByFile")]
    public async Task<IActionResult> ExactSearchByFileAsync([FromForm] SearchByFileModel model)
    {
        var validationResult = await new SearchByFileModelValidator().ValidateAsync(model);
        if (!validationResult.IsValid)
        {
            return Error.Create(400, validationResult);
        }

        await using var fileStream = model.File.OpenReadStream();

        var (thumbnail, thumbnailError) = await CreateThumbnailAsync(fileStream);
        if (thumbnail == null)
        {
            return thumbnailError.AsResult();
        }

        using var thumbnailStream = new MemoryStream(thumbnail);
        var searchModels = await ExactSearchAsync(thumbnailStream, model.Limit);

        var requestId = $"{ShortUuidDateTime.ToString(DateTime.UtcNow)}{ShortUuid.Random(12)}";
        return Ok(new SearchResultsModel
        {
            Id = requestId,
            Results = searchModels
        });
    }

    private async Task<(byte[]? thumbnail, ImagingErrorCode? errorCode)> CreateThumbnailAsync(Stream stream)
    {
        try
        {
            var (thumbnail, _) = await _imagingApiClient.CreateThumbnailAsync(stream, size: 300, quality: 95, calculateCenter: false);
            return (thumbnail, null);
        }
        catch (ImagingApiException e)
        {
            return (null, e.Code);
        }
    }

    private async Task<List<SearchResultModel>> ExactSearchAsync(Stream stream, int limit)
    {
        var vectors = await _inferenceApiClient.ExactMatchV2Async([stream]);
        var vector = vectors[0];

        var vectorSearchResults = await _vectorApiClient.SearchCollectionAsync("exactMatchV2", new VectorSearchParametersModel
        {
            Query = vector,
            Limit = limit + 4 // An extra 4 so that D2-D5 are filled with real values for the last item
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

        var searchModels = items
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
            .Take(limit)
            .ToList();

        var probableModels = searchModels
            .Where(x => x.Match == SearchResultModelMatch.Exact)
            .GroupBy(x => x.Platform)
            .SelectMany(x => x.OrderByDescending(x => x.Distance).Skip(1).ToList());

        foreach (var probableModel in probableModels)
        {
            probableModel.Match = SearchResultModelMatch.Probable;
        }

        return searchModels;
    }
}
