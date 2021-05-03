using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Noppes.Fluffle.Search.Api.Models;
using Noppes.Fluffle.Search.Api.Services;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Search.Api.Controllers
{
    public class SearchApiController : SearchApiControllerV1
    {
        private readonly ISearchService _searchService;

        public SearchApiController(ISearchService searchService)
        {
            _searchService = searchService;
        }

        [AllowAnonymous]
        [HttpPost("search")]
        public async Task<IActionResult> Search([FromForm] SearchModel model)
        {
            var result = await _searchService.SearchAsync(model);

            return HandleV1(result);
        }
    }
}
