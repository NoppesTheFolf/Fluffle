using Fluffle.Vector.Api.Authentication;
using Fluffle.Vector.Core;
using Fluffle.Vector.Mongo;
using Fluffle.Vector.Qdrant;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

services.AddCore();

services.AddMongo();

services.AddQdrant();

services.AddApiKey();

services.AddControllers();

var app = builder.Build();

app.UseApiKey();

app.MapControllers();

app.Run();
