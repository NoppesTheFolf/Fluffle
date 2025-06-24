using Fluffle.Imaging.Api.Client;
using Fluffle.Inference.Api.Client;
using Fluffle.Search.Api.Validation;
using Fluffle.Vector.Api.Client;
using Fluffle_Search_Api;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.ML;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

services.AddPredictionEnginePool<ExactMatchV2IsMatch.ModelInput, ExactMatchV2IsMatch.ModelOutput>()
    .FromFile("ML/ExactMatchV2IsMatch.mlnet");

services.AddImagingApiClient();

services.AddInferenceApiClient();

services.AddVectorApiClient();

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
                .WithOrigins("https://fluffle.xyz")
                .AllowAnyMethod();
        }
    });
});

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
                QueueLimit = 32,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst
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

var app = builder.Build();

app.UseCors();

app.UseForwardedHeaders();

app.UseRateLimiter();

app.UseMiddleware<RequireUserAgentMiddleware>();

app.MapControllers();

app.Run();
