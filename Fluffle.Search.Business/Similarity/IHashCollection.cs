namespace Noppes.Fluffle.Search.Business.Similarity;

internal interface IHashCollection
{
    void Add(int id, ulong hash64, ReadOnlySpan<ulong> hash256);

    NearestNeighborsResults NearestNeighbors(ulong hash64, ulong threshold64, ReadOnlySpan<ulong> hash256, int k);
}
