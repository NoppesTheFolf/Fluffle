using Fluffle.Imaging.Api.Client;
using Fluffle.Inference.Api.Client;
using Fluffle.Vector.Api.Client;
using Microsoft.AspNetCore.HttpOverrides;
using System.Net;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

services.AddImagingApiClient();

services.AddInferenceApiClient();

services.AddVectorApiClient();

services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor;
});

services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetConcurrencyLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new ConcurrencyLimiterOptions
            {
                PermitLimit = 1,
                QueueLimit = 4,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst
            }));
    options.RejectionStatusCode = (int)HttpStatusCode.TooManyRequests;
});

services.AddControllers();

var app = builder.Build();

app.UseForwardedHeaders();

app.UseRateLimiter();

app.MapControllers();

app.Run();
