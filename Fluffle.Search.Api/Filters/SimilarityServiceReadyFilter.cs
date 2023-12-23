using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Noppes.Fluffle.Api;
using Noppes.Fluffle.Search.Business.Similarity;
using System.Net;

namespace Noppes.Fluffle.Search.Api.Filters;

public class SimilarityServiceReadyFilter : IActionFilter
{
    public static readonly V1Error StartingUpError = new("UNAVAILABLE", "The server isn't ready to handle requests yet.");

    private readonly ISimilarityService _similarityService;

    public SimilarityServiceReadyFilter(ISimilarityService similarityService)
    {
        _similarityService = similarityService;
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (_similarityService.IsReady)
            return;

        context.Result = new ObjectResult(StartingUpError)
        {
            StatusCode = (int)HttpStatusCode.ServiceUnavailable
        };
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
    }
}
