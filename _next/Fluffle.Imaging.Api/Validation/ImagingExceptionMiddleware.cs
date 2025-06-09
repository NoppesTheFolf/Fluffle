using Fluffle.Imaging.Api.Models;

namespace Fluffle.Imaging.Api.Validation;

public class ImagingExceptionMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (ImagingException e)
        {
            context.Response.StatusCode = 400; // Bad Request
            await context.Response.WriteAsJsonAsync(new ImagingErrorModel
            {
                Code = e.Code,
                Message = e.Message
            });
        }
    }
}
