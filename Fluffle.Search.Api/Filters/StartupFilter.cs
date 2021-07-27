using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Noppes.Fluffle.Api;
using System.Net;

namespace Noppes.Fluffle.Search.Api.Filters
{
    public class StartupFilter : IActionFilter
    {
        public static volatile bool IsStarting = true;

        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (!IsStarting)
                return;

            context.Result = new OkObjectResult(new V1Error("UNAVAILABLE", "The server isn't ready to handle requests yet."))
            {
                StatusCode = (int)HttpStatusCode.ServiceUnavailable
            };
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }
    }
}
