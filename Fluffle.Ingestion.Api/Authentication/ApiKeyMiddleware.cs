using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace Fluffle.Ingestion.Api.Authentication;

public class ApiKeyMiddleware : IMiddleware
{
    private readonly IOptions<ApiKeyOptions> _options;

    public ApiKeyMiddleware(IOptions<ApiKeyOptions> options)
    {
        _options = options;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (!context.Request.Headers.TryGetValue("Api-Key", out var values))
        {
            await SetUnauthorizedAsync(context);
            return;
        }

        var value = values.FirstOrDefault() ?? string.Empty;
        if (!CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(value), Encoding.UTF8.GetBytes(_options.Value.Value)))
        {
            await SetUnauthorizedAsync(context);
            return;
        }

        await next(context);
    }

    private static async Task SetUnauthorizedAsync(HttpContext context)
    {
        context.Response.StatusCode = 401;
        await context.Response.WriteAsJsonAsync("Invalid API key.");
    }
}
