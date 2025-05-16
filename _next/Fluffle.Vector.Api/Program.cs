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

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseApiKey();

app.MapControllers();

app.Run();
