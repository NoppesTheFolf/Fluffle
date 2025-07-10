using Fluffle.Content.Api.Authentication;
using Fluffle.Content.Api.Storage;
using Microsoft.AspNetCore.HttpOverrides;
using System.Net;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

services.AddApiKey();

services.AddOptions<FtpStorageOptions>()
    .BindConfiguration(FtpStorageOptions.FtpStorage)
    .ValidateDataAnnotations().ValidateOnStart();

services.AddSingleton<FtpClientPool>();
services.AddSingleton<FtpStorage>();

services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            policy.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        }
        else
        {
            policy
                .WithOrigins("https://fluffle.xyz", "https://*.fluffle.xyz")
                .SetIsOriginAllowedToAllowWildcardSubdomains()
                .AllowAnyMethod();
        }
    });
});

services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor;
});

var concurrencyLimit = builder.Configuration.GetValue<int>("Concurrency:Limit");
var concurrencyQueueSize = builder.Configuration.GetValue<int>("Concurrency:QueueSize");
services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetConcurrencyLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new ConcurrencyLimiterOptions
            {
                PermitLimit = concurrencyLimit,
                QueueLimit = concurrencyQueueSize,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst
            }));
    options.RejectionStatusCode = (int)HttpStatusCode.TooManyRequests;
});

services.AddControllers();

var app = builder.Build();

app.UseForwardedHeaders();

app.UseCors();

app.UseApiKey();

app.UseRateLimiter();

app.MapControllers();

app.Run();
