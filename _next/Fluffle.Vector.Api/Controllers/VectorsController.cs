using Fluffle.Vector.Api.Models.Vectors;
using Fluffle.Vector.Core.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Fluffle.Vector.Api.Controllers;

[ApiController]
public class VectorsController : ControllerBase
{
    private readonly IItemVectorsRepository _itemVectorsRepository;
    private readonly IModelRepository _modelRepository;

    public VectorsController(IItemVectorsRepository itemVectorsRepository, IModelRepository modelRepository)
    {
        _itemVectorsRepository = itemVectorsRepository;
        _modelRepository = modelRepository;
    }

    [HttpPost("/vectors/search", Name = "SearchVectors")]
    public async Task<IActionResult> SearchVectorsAsync([FromBody] VectorSearchParametersModel dto)
    {
        var model = await _modelRepository.GetAsync(dto.ModelId);
        if (model == null)
        {
            return NotFound($"No model with ID '{dto.ModelId}' could be found.");
        }

        if (dto.Query.Length != model.VectorDimensions)
        {
            return BadRequest($"Query length of vector does not equal expected vector length of model ({model.VectorDimensions}).");
        }

        var searchResults = await _itemVectorsRepository.GetAsync(dto.ModelId, dto.Query, dto.Limit);
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
