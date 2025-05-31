using Fluffle.Ingestion.Api.Authentication;
using Fluffle.Ingestion.Core;
using Fluffle.Ingestion.Mongo;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

services.AddCore();

services.AddMongo();

services.AddApiKey();

services.AddControllers();

var app = builder.Build();

app.UseApiKey();

app.MapControllers();

app.Run();
