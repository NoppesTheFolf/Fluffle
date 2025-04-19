using Fluffle.Content.Api.Storage;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

services.AddOptions<FtpStorageOptions>()
    .BindConfiguration(FtpStorageOptions.FtpStorage)
    .ValidateDataAnnotations().ValidateOnStart();

services.AddSingleton<FtpClientPool>();
services.AddSingleton<FtpStorage>();

services.AddControllers();

var app = builder.Build();

app.MapControllers();

app.Run();
