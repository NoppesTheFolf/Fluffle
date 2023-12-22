namespace Noppes.Fluffle.Search.Business.Similarity;

internal interface IHashCollection
{
    void Add(int id, ulong hash64, ReadOnlySpan<ulong> hash256);

    bool TryRemove(int id);

    NearestNeighborsResults NearestNeighbors(ulong hash64, ulong threshold64, ReadOnlySpan<ulong> hash256, int k);

    Task SerializeAsync(Stream stream);

    Task DeserializeAsync(Stream stream);
}
