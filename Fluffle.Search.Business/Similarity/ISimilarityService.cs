namespace Noppes.Fluffle.Search.Business.Similarity;

public interface ISimilarityService
{
    bool IsReady { get; }

    Task RefreshAsync();

    IDictionary<int, SimilarityResult> NearestNeighbors(ulong hash64, ReadOnlySpan<ulong> hash256, bool includeNsfw, int limit);

    Task CreateDumpAsync();

    Task<bool> TryRestoreDumpAsync();
}
