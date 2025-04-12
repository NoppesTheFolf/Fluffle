using Fluffle.Vector.Core.Repositories;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Fluffle.Vector.Core.Vectors;

internal class VectorCollectionInitializer : BackgroundService
{
    private readonly IItemVectorsRepository _itemVectorsRepository;
    private readonly VectorCollection _collection;
    private readonly ILogger<VectorCollectionInitializer> _logger;

    public VectorCollectionInitializer(IItemVectorsRepository itemVectorsRepository, VectorCollection collection, ILogger<VectorCollectionInitializer> logger)
    {
        _itemVectorsRepository = itemVectorsRepository;
        _collection = collection;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // TODO: We'll make this more flexible when the time comes
                _logger.LogInformation("Start initializing vector collection...");
                await _itemVectorsRepository.ForEachAsync("exactMatchV1", itemVectors =>
                {
                    foreach (var itemVector in itemVectors.Vectors)
                    {
                        _collection.Add(itemVectors.ItemVectorsId.ItemId, itemVector.Value);
                    }
                }, stoppingToken);
                _logger.LogInformation("Initialization of the vector collection is done!");

                break;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Something went wrong while initializing the vector collection.");
            }

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}
