using Fluffle.Imaging.Api.Authentication;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

services.AddApiKey();

services.AddControllers();

var app = builder.Build();

app.UseApiKey();

app.MapControllers();

app.Run();
