using Fluffle.Imaging.Api.Client;
using Fluffle.Inference.Api.Client;
using Fluffle.Ingestion.Api.Client;
using Fluffle.Ingestion.Worker;
using Fluffle.Ingestion.Worker.ItemActionHandlers;
using Fluffle.Ingestion.Worker.ItemContentClient;
using Fluffle.Ingestion.Worker.ThumbnailStorage;
using Fluffle.Vector.Api.Client;

var builder = Host.CreateApplicationBuilder(args);
var services = builder.Services;

services.AddIngestionApiClient();

services.AddImagingApiClient();

services.AddInferenceApiClient();

services.AddVectorApiClient();

services.AddSingleton<IItemContentClient, ItemContentClient>();

services.AddOptions<FtpThumbnailStorageOptions>()
    .BindConfiguration(FtpThumbnailStorageOptions.FtpThumbnailStorage)
    .ValidateDataAnnotations().ValidateOnStart();
services.AddSingleton<IThumbnailStorage, FtpThumbnailStorage>();

services.AddSingleton<ItemActionHandlerFactory>();

services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
