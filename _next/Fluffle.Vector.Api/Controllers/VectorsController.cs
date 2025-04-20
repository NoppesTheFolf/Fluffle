using Fluffle.Vector.Api.Models.Vectors;
using Fluffle.Vector.Core.Vectors;
using Microsoft.AspNetCore.Mvc;

namespace Fluffle.Vector.Api.Controllers;

[ApiController]
public class VectorsController : ControllerBase
{
    private readonly VectorCollection _collection;

    public VectorsController(VectorCollection collection)
    {
        _collection = collection;
    }

    [HttpPost("/vectors/search", Name = "SearchVectors")]
    public IActionResult SearchVectorsAsync([FromBody] VectorSearchParametersModel model)
    {
        var searchResult = _collection.Search(model.Query, model.Limit);
        var searchResultModels = searchResult.Select(x => new VectorSearchResultModel
        {
            ItemId = x.ItemId,
            Distance = x.Distance
        }).ToList();

        return Ok(searchResultModels);
    }
}
