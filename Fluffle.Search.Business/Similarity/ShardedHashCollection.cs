using Noppes.Fluffle.Utils;

namespace Noppes.Fluffle.Search.Business.Similarity;

internal class ShardedHashCollection : IHashCollection
{
    private const int InitialSize = 5_000;
    private const int ResizeStepSize = 5_000;

    private readonly int _shardsCount;
    private readonly HashCollection[] _hashCollections;

    public ShardedHashCollection(int shardsCount)
    {
        _shardsCount = shardsCount;

        _hashCollections = new HashCollection[shardsCount];
        for (var i = 0; i < _shardsCount; i++)
            _hashCollections[i] = new HashCollection(InitialSize, ResizeStepSize);
    }

    public void Add(int id, ulong hash64, ReadOnlySpan<ulong> hash256) => GetHashCollection(id).Add(id, hash64, hash256);

    public bool TryRemove(int id) => GetHashCollection(id).TryRemove(id);

    public HashCollection GetHashCollection(int id)
    {
        var index = id.GetHashCode() % _shardsCount;
        var hashCollection = _hashCollections[index];

        return hashCollection;
    }

    public NearestNeighborsStats NearestNeighbors(TopNList<NearestNeighborsResult> results, ulong hash64, ulong threshold64, ReadOnlySpan<ulong> hash256)
    {
        var count64 = 0;
        var count256 = 0;
        foreach (var hashCollection in _hashCollections)
        {
            var sharedStats = hashCollection.NearestNeighbors(results, hash64, threshold64, hash256);

            count64 += sharedStats.Count64;
            count256 += sharedStats.Count256;
        }

        return new NearestNeighborsStats(count64, count256);
    }

    public async Task DeserializeAsync(Stream stream)
    {
        foreach (var hashCollection in _hashCollections)
            await hashCollection.DeserializeAsync(stream);
    }

    public async Task SerializeAsync(Stream stream)
    {
        foreach (var hashCollection in _hashCollections)
            await hashCollection.SerializeAsync(stream);
    }
}
