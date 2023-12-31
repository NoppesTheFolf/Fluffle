using Noppes.Fluffle.Utils;

namespace Noppes.Fluffle.Search.Business.Similarity;

internal interface IHashCollection
{
    void Add(int id, ulong hash64, ReadOnlySpan<ulong> hash256);

    bool TryRemove(int id);

    NearestNeighborsStats NearestNeighbors(TopNList<NearestNeighborsResult> results, ulong hash64, ulong threshold64, ReadOnlySpan<ulong> hash256);

    Task SerializeAsync(Stream stream);

    Task DeserializeAsync(Stream stream);
}
