using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

namespace Noppes.Fluffle.Search.Api.Filters
{
    public class RequireUserAgentFilter : IActionFilter
    {
        private readonly IOptions<ApiBehaviorOptions> _apiBehaviorOptions;

        public RequireUserAgentFilter(IOptions<ApiBehaviorOptions> apiBehaviorOptions)
        {
            _apiBehaviorOptions = apiBehaviorOptions;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.HttpContext.Request.Headers.TryGetValue("User-Agent", out var userAgent) && !string.IsNullOrEmpty(userAgent))
                return;

            context.ModelState.AddModelError("Headers", "The User-Agent header is required when making a search request. See https://fluffle.xyz/api for more information.");
            context.Result = _apiBehaviorOptions.Value.InvalidModelStateResponseFactory(context);
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }
    }
}
