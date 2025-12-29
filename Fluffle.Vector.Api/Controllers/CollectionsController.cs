using Fluffle.Vector.Api.Models.Vectors;
using Fluffle.Vector.Core.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Fluffle.Vector.Api.Controllers;

[ApiController]
public class CollectionsController : ControllerBase
{
    private readonly IItemVectorsRepository _itemVectorsRepository;
    private readonly ICollectionRepository _collectionRepository;

    public CollectionsController(IItemVectorsRepository itemVectorsRepository, ICollectionRepository collectionRepository)
    {
        _itemVectorsRepository = itemVectorsRepository;
        _collectionRepository = collectionRepository;
    }

    [HttpPost("/collections/{collectionId}/search", Name = "SearchVectors")]
    public async Task<IActionResult> SearchVectorsAsync(string collectionId, [FromBody] VectorSearchParametersModel dto)
    {
        var collection = await _collectionRepository.GetAsync(collectionId);
        if (collection == null)
        {
            return NotFound($"No collection with ID '{collectionId}' could be found.");
        }

        if (dto.Query.Length != collection.VectorDimensions)
        {
            return BadRequest($"Query length of vector does not equal expected vector length of collection ({collection.VectorDimensions}).");
        }

        var searchResults = await _itemVectorsRepository.GetAsync(collectionId, dto.Query, dto.Limit);
        var searchResultModels = searchResults.Select(x => new VectorSearchResultModel
        {
            ItemId = x.ItemId,
            Distance = x.Distance,
            Properties = JsonSerializer.SerializeToNode(x.Properties) ??
                         throw new InvalidOperationException("Vector properties should never serialize to null.")
        }).ToList();

        return Ok(searchResultModels);
    }
}
