using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Noppes.Fluffle.Search.Business.Repositories;
using Noppes.Fluffle.Search.Domain;
using System.Diagnostics;

namespace Noppes.Fluffle.Search.Business.Similarity;

public interface ISimilarityService
{
    IDictionary<int, SimilarityResult> NearestNeighbors(ulong hash64, ReadOnlySpan<ulong> hash256, bool includeNsfw, int limit);

    Task RefreshAsync();
}

public class SimilarityService : ISimilarityService
{
    private const int NnThreshold = 18;
    private const int BatchSize = 25_000;
    private const int NextPlatformDelay = 2500;

    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SimilarityService> _logger;
    private readonly IDictionary<int, (HashCollection sfw, HashCollection nsfw)> _hashCollections;
    private readonly IDictionary<int, long> _changeIds;

    public SimilarityService(IServiceProvider serviceProvider, ILogger<SimilarityService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _hashCollections = new Dictionary<int, (HashCollection, HashCollection)>();
        _changeIds = new Dictionary<int, long>();
    }

    public IDictionary<int, SimilarityResult> NearestNeighbors(ulong hash64, ReadOnlySpan<ulong> hash256, bool includeNsfw, int limit)
    {
        var stopwatch = Stopwatch.StartNew();

        var result = new Dictionary<int, SimilarityResult>();
        foreach (var (platformId, platformHashCollections) in _hashCollections)
        {
            IEnumerable<HashCollection> hashCollections = new[] { platformHashCollections.sfw };
            if (includeNsfw)
                hashCollections = hashCollections.Concat(new[] { platformHashCollections.nsfw });

            var count = 0;
            var nnResults = new List<NearestNeighborsResult>();
            foreach (var hashCollection in hashCollections)
            {
                var nnResult = hashCollection.NearestNeighbors(hash64, NnThreshold, hash256, limit);
                _logger.LogTrace("Searched through {count64}/{count256} hashes on platform with ID {platformId}", nnResult.Count64, nnResult.Count256, platformId);

                count += nnResult.Count64;
                nnResults.AddRange(nnResult.Results);
            }

            result[platformId] = new SimilarityResult(count, nnResults);
        }

        var totalCount = result.Values.Sum(x => x.Count);
        _logger.LogInformation("Performed similarity search on {count} images in {time}ms", totalCount, stopwatch.ElapsedMilliseconds);

        return result;
    }

    public async Task RefreshAsync()
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var platformRepository = scope.ServiceProvider.GetRequiredService<IPlatformRepository>();

        // Initialize hash collection per platform
        var platforms = await platformRepository.GetAsync();
        foreach (var platform in platforms)
        {
            if (_hashCollections.ContainsKey(platform.Id))
                continue;

            _hashCollections[platform.Id] = (new HashCollection(), new HashCollection());
            _changeIds[platform.Id] = 0;
        }

        // Start the refresh process for all platforms
        var tasks = new List<Task>();
        foreach (var platform in platforms)
        {
            var task = Task.Run(async () => await RefreshAsync(platform));
            tasks.Add(task);

            await Task.Delay(NextPlatformDelay);
        }

        // Wait for all refreshes to complete
        await Task.WhenAll(tasks);

        var exceptions = tasks
            .Where(x => x.Exception != null)
            .Select(x => x.Exception)
            .Cast<AggregateException>()
            .ToList();

        if (exceptions.Count > 0)
            throw new AggregateException("Something went wrong while refreshes hashes", exceptions);
    }

    private async Task RefreshAsync(Platform platform)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var imageRepository = scope.ServiceProvider.GetRequiredService<IImageRepository>();

        _logger.LogInformation("{platform} | Starting to refresh hashes", platform.Name);
        var (sfwCollection, nsfwCollection) = _hashCollections[platform.Id];
        var changeId = _changeIds[platform.Id];

        while (true)
        {
            var images = await imageRepository.GetAsync(platform.Id, changeId, BatchSize);

            if (images.Count > 0)
            {
                _logger.LogInformation("{platformName} | Updating {count} hashes after change ID {changeId}", platform.Name, images.Count, changeId);

                foreach (var image in images)
                {
                    var hashCollection = image.IsSfw ? sfwCollection : nsfwCollection;
                    hashCollection.Add(image.Id, image.PhashAverage64, image.PhashAverage256);
                }

                changeId = images.Select(x => x.ChangeId).Max(x => x);
            }

            if (images.Count < BatchSize)
            {
                _logger.LogInformation("{platformName} | No more images can be retrieved after change ID {changeId}", platform.Name, changeId);
                _changeIds[platform.Id] = changeId;

                return;
            }
        }
    }
}
