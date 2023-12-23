using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nito.AsyncEx;
using Noppes.Fluffle.Search.Business.Repositories;
using Noppes.Fluffle.Search.Domain;
using System.Diagnostics;

namespace Noppes.Fluffle.Search.Business.Similarity;

internal class SimilarityService : ISimilarityService
{
    private const int NnThreshold = 18;
    private const int BatchSize = 25_000;
    private const int NextPlatformDelay = 2500;

    private readonly object _isReadyLock = new();
    private bool _isReady;
    public bool IsReady
    {
        get
        {
            lock (_isReadyLock)
                return _isReady;
        }
        private set
        {
            lock (_isReadyLock)
                _isReady = value;
        }
    }

    private readonly ISimilarityDataSerializer _serializer;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SimilarityService> _logger;

    private Dictionary<int, PlatformSimilarityData> _data;
    private readonly AsyncLock _lock;

    public SimilarityService(ISimilarityDataSerializer serializer, IServiceProvider serviceProvider, ILogger<SimilarityService> logger)
    {
        _serializer = serializer;
        _serviceProvider = serviceProvider;
        _logger = logger;

        _data = new Dictionary<int, PlatformSimilarityData>();
        _lock = new AsyncLock();
    }

    public async Task<SimilarityDataDump?> TryRestoreDumpAsync()
    {
        var dumps = await _serializer.GetDumpsAsync();
        foreach (var dump in dumps.OrderByDescending(x => x.When))
        {
            _logger.LogInformation("Attempting to restore dump with ID {id} made at {when}", dump.Id, dump.When);

            try
            {
                var stopwatch = Stopwatch.StartNew();

                var data = await _serializer.RestoreDumpAsync(dump);
                _data = data.ToDictionary(x => x.PlatformId);
                _logger.LogInformation("Dump with ID {id} restored in {time}ms", dump.Id, stopwatch.ElapsedMilliseconds);

                IsReady = true;
                return dump;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Something went wrong trying to restore dump with ID {id}", dump.Id);
            }
        }

        return null;
    }

    public async Task CreateDumpAsync()
    {
        using var _ = await _lock.LockAsync();

        await _serializer.CreateDumpAsync(_data.Values);

        var dumps = await _serializer.GetDumpsAsync();
        var redundantDumps = dumps.OrderByDescending(x => x.When).Skip(2).ToList();
        foreach (var redundantDump in redundantDumps)
        {
            await _serializer.TryPurgeDumpAsync(redundantDump);
        }
    }

    public IDictionary<int, SimilarityResult> NearestNeighbors(ulong hash64, ReadOnlySpan<ulong> hash256, bool includeNsfw, int limit)
    {
        var stopwatch = Stopwatch.StartNew();

        var result = new Dictionary<int, SimilarityResult>();
        foreach (var item in _data.Values)
        {
            IEnumerable<IHashCollection> hashCollections = new[] { item.SfwCollection };
            if (includeNsfw)
                hashCollections = hashCollections.Concat(new[] { item.NsfwCollection });

            var count = 0;
            var nnResults = new List<NearestNeighborsResult>();
            foreach (var hashCollection in hashCollections)
            {
                var nnResult = hashCollection.NearestNeighbors(hash64, NnThreshold, hash256, limit);
                _logger.LogTrace("Searched through {count64}/{count256} hashes on platform with ID {platformId}", nnResult.Count64, nnResult.Count256, item.PlatformId);

                count += nnResult.Count64;
                nnResults.AddRange(nnResult.Results);
            }

            result[item.PlatformId] = new SimilarityResult(count, nnResults);
        }

        var totalCount = result.Values.Sum(x => x.Count);
        _logger.LogInformation("Performed similarity search on {count} images in {time}ms", totalCount, stopwatch.ElapsedMilliseconds);

        return result;
    }

    public async Task RefreshAsync()
    {
        using var _ = await _lock.LockAsync();

        await using var scope = _serviceProvider.CreateAsyncScope();
        var platformRepository = scope.ServiceProvider.GetRequiredService<IPlatformRepository>();

        // Initialize hash collection per platform
        var platforms = await platformRepository.GetAsync();
        foreach (var platform in platforms)
        {
            if (_data.ContainsKey(platform.Id))
                continue;

            _data[platform.Id] = new PlatformSimilarityData
            {
                PlatformId = platform.Id,
                ChangeId = 0,
                SfwCollection = HashCollectionFactory.Create(),
                NsfwCollection = HashCollectionFactory.Create()
            };
        }

        // Start the refresh process for all platforms
        var tasks = new List<Task>();
        foreach (var platform in platforms)
        {
            var task = Task.Run(async () => await RefreshAsync(platform));
            tasks.Add(task);

            await Task.WhenAny(task, Task.Delay(NextPlatformDelay));
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

        IsReady = true;
    }

    private async Task RefreshAsync(Platform platform)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var imageRepository = scope.ServiceProvider.GetRequiredService<IImageRepository>();

        _logger.LogInformation("{platform} | Starting to refresh hashes", platform.Name);
        var item = _data[platform.Id];
        var changeId = item.ChangeId;

        while (true)
        {
            var images = await imageRepository.GetAsync(platform.Id, changeId, BatchSize);

            if (images.Count > 0)
            {
                _logger.LogInformation("{platformName} | Updating {count} hashes after change ID {changeId}", platform.Name, images.Count, changeId);

                foreach (var image in images)
                {
                    item.SfwCollection.TryRemove(image.Id);
                    item.NsfwCollection.TryRemove(image.Id);
                    if (image.IsDeleted)
                        continue;

                    var hashCollection = image.IsSfw ? item.SfwCollection : item.NsfwCollection;
                    hashCollection.Add(image.Id, image.PhashAverage64, image.PhashAverage256);
                }

                changeId = images.Select(x => x.ChangeId).Max(x => x);
            }

            if (images.Count < BatchSize)
            {
                _logger.LogInformation("{platformName} | No more images can be retrieved after change ID {changeId}", platform.Name, changeId);
                _data[platform.Id].ChangeId = changeId;

                return;
            }
        }
    }
}
