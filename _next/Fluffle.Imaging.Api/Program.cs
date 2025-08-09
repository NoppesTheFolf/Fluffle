using Fluffle.Imaging.Api;
using Fluffle.Imaging.Api.Authentication;
using Fluffle.Imaging.Api.Validation;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;

// Prevent a single image from taking up a lot of resources
NetVips.NetVips.Concurrency = 1;

// Significantly reduces memory processing large images
NetVips.Cache.Max = 0;
NetVips.Cache.MaxMem = 0;
NetVips.Cache.MaxFiles = 0;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

var maxRequestBodySize = builder.Configuration.GetRequiredSection("MaxRequestBodySize").Get<int>();
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = maxRequestBodySize;
});

services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
});

services.AddOptions<ImagingOptions>()
    .BindConfiguration(ImagingOptions.Imaging)
    .ValidateDataAnnotations().ValidateOnStart();

services.AddApiKey();

var concurrencyLimit = builder.Configuration.GetValue<int>("Concurrency:Limit");
var concurrencyQueueSize = builder.Configuration.GetValue<int>("Concurrency:QueueSize");
services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(_ =>
        RateLimitPartition.GetConcurrencyLimiter(
            partitionKey: string.Empty,
            factory: _ => new ConcurrencyLimiterOptions
            {
                PermitLimit = concurrencyLimit,
                QueueLimit = concurrencyQueueSize,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst
            }));
    options.RejectionStatusCode = (int)HttpStatusCode.TooManyRequests;
});

services.AddControllers();

services.AddValidation();

var app = builder.Build();

app.UseApiKey();

app.UseRateLimiter();

app.UseValidation();

app.MapControllers();

app.Run();
