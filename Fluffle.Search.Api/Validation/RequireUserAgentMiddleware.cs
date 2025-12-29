namespace Fluffle.Search.Api.Validation;

public class RequireUserAgentMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (context.Request.Headers.TryGetValue("User-Agent", out var userAgent) && !string.IsNullOrEmpty(userAgent))
        {
            await next(context);
            return;
        }

        context.Response.StatusCode = 400;
        await context.Response.WriteAsJsonAsync(new ErrorResponseModel
        {
            Errors = new List<ErrorModel>
            {
                new()
                {
                    Code = null,
                    Message = "The User-Agent header is required when making a request. See https://fluffle.xyz/api for more information."
                }
            }
        });
    }
}
