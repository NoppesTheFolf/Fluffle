using Fluffle.Ingestion.Api.Authentication;
using Fluffle.Ingestion.Core;
using Fluffle.Ingestion.Database;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

services.AddCore();

services.AddMongo();

services.AddApiKey();

services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseApiKey();

app.MapControllers();

app.Run();
