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

    public NearestNeighborsResults NearestNeighbors(ulong hash64, ulong threshold64, ReadOnlySpan<ulong> hash256, int k)
    {
        var count64 = 0;
        var count256 = 0;
        var results = new List<NearestNeighborsResult>();
        foreach (var hashCollection in _hashCollections)
        {
            var sharedResults = hashCollection.NearestNeighbors(hash64, threshold64, hash256, k);

            count64 += sharedResults.Count64;
            count256 += sharedResults.Count256;
            results.AddRange(sharedResults.Results);
        }

        return new NearestNeighborsResults(count64, count256, results);
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
