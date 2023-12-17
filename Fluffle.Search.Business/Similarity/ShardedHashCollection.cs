namespace Noppes.Fluffle.Search.Business.Similarity;

internal class ShardedHashCollection : IHashCollection
{
    private const int ShardInitialSize = 0;
    private const int ShardResizeStepSize = 5_000;

    private readonly int _shardsCount;
    private readonly HashCollection[] _hashCollections;

    public ShardedHashCollection(int shardsCount)
    {
        _shardsCount = shardsCount;

        _hashCollections = new HashCollection[shardsCount];
        for (var i = 0; i < _hashCollections.Length; i++)
            _hashCollections[i] = new HashCollection(ShardInitialSize, ShardResizeStepSize);
    }

    public void Add(int id, ulong hash64, ReadOnlySpan<ulong> hash256)
    {
        var index = id.GetHashCode() % _shardsCount;

        var hashCollection = _hashCollections[index];
        hashCollection.Add(id, hash64, hash256);
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
}
