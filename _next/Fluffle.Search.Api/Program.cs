using Fluffle.Content.Api.Client;
using Fluffle.Imaging.Api.Client;
using Fluffle.Inference.Api.Client;
using Fluffle.Search.Api.OpenApi;
using Fluffle.Search.Api.SearchByUrl;
using Fluffle.Search.Api.Validation;
using Fluffle.Vector.Api.Client;
using Fluffle_Search_Api;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.ML;
using System.Net;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using IPNetwork = Microsoft.AspNetCore.HttpOverrides.IPNetwork;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

services.AddPredictionEnginePool<ExactMatchV2IsMatch.ModelInput, ExactMatchV2IsMatch.ModelOutput>()
    .FromFile("ML/ExactMatchV2IsMatch.mlnet");

if (Assembly.GetEntryAssembly()?.GetName().Name != "GetDocument.Insider")
{
    services.AddImagingApiClient();

    services.AddInferenceApiClient();

    services.AddVectorApiClient();

    services.AddContentApiClient();
}

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
                .WithOrigins("https://fluffle.xyz", "https://*.fluffle.xyz", "https://fluffle.pages.dev", "https://*.fluffle.pages.dev")
                .SetIsOriginAllowedToAllowWildcardSubdomains()
                .AllowAnyMethod();
        }
    });
});

services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor;
    options.KnownNetworks.Add(IPNetwork.Parse("0.0.0.0/0"));
    options.KnownNetworks.Add(IPNetwork.Parse("::/0"));
});

services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetConcurrencyLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new ConcurrencyLimiterOptions
            {
                PermitLimit = 1,
                QueueLimit = 32,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst
            }));

    options.AddPolicy("exact-search-by-url", httpContext => RateLimitPartition.GetTokenBucketLimiter(
        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        factory: _ => new TokenBucketRateLimiterOptions
        {
            AutoReplenishment = true,
            TokenLimit = 8,
            TokensPerPeriod = 1,
            ReplenishmentPeriod = TimeSpan.FromSeconds(5),
            QueueLimit = 4,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
        }));

    options.RejectionStatusCode = (int)HttpStatusCode.TooManyRequests;
});
services.AddSingleton<RequireUserAgentMiddleware>();

services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});

services.AddControllers(options =>
{
    options.Filters.Add<CustomModelStateInvalidFilter>();
}).AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
});

services.AddHttpClient(nameof(SafeDownloadClient), client =>
{
    client.DefaultRequestHeaders.Add("User-Agent", "fluffle.xyz by NoppesTheFolf");
}).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    AllowAutoRedirect = false,
    UseCookies = false
});
services.AddSingleton<SafeDownloadClient>();

services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<FluffleDocumentTransformer>();
    options.AddSchemaTransformer<JsonStringEnumSchemaTransformer>();
});

var app = builder.Build();

app.UseForwardedHeaders();

app.UseCors();

app.UseRateLimiter();

app.UseMiddleware<RequireUserAgentMiddleware>();

app.MapControllers();

app.MapOpenApi();

app.Run();
