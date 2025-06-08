using Fluffle.Imaging.Api.Client;
using Fluffle.Inference.Api.Client;
using Fluffle.Search.Api.Models;
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
        await using var imageStream = model.Image.OpenReadStream();
        var (thumbnail, _) = await _imagingApiClient.CreateThumbnailAsync(imageStream, size: 300, quality: 95, calculateCenter: false);

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

                string? thumbnailUrl = null;
                if (x.Thumbnail != null)
                {
                    var thumbnailPath = Path.GetRelativePath("/fluffle", new Uri(x.Thumbnail.Url).AbsolutePath);
                    var thumbnailUrlBuilder = new UriBuilder("https", "content.fluffle.xyz", 443, thumbnailPath);
                    thumbnailUrl = thumbnailUrlBuilder.Uri.AbsoluteUri;
                }

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
                            Url = thumbnailUrl!
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
