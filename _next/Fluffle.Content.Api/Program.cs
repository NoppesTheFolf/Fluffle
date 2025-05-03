using Fluffle.Content.Api.Storage;
using Microsoft.AspNetCore.HttpOverrides;
using System.Net;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

services.AddOptions<FtpStorageOptions>()
    .BindConfiguration(FtpStorageOptions.FtpStorage)
    .ValidateDataAnnotations().ValidateOnStart();

services.AddSingleton<FtpClientPool>();
services.AddSingleton<FtpStorage>();

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
                PermitLimit = 2,
                QueueLimit = 50,
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
