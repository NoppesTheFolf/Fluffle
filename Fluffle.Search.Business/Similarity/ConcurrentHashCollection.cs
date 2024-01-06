using Nito.AsyncEx;
using Noppes.Fluffle.Utils;

namespace Noppes.Fluffle.Search.Business.Similarity;

internal class ConcurrentHashCollection : IHashCollection
{
    private readonly IHashCollection _hashCollection;
    private readonly AsyncReaderWriterLock _lock;

    public ConcurrentHashCollection(IHashCollection hashCollection)
    {
        _hashCollection = hashCollection;
        _lock = new AsyncReaderWriterLock();
    }

    public void Add(int id, ulong hash64, ReadOnlySpan<ulong> hash256)
    {
        using var _ = _lock.WriterLock();

        _hashCollection.Add(id, hash64, hash256);
    }

    public bool TryRemove(int id)
    {
        using var _ = _lock.WriterLock();

        return _hashCollection.TryRemove(id);
    }

    public NearestNeighborsStats NearestNeighbors(TopNList<NearestNeighborsResult> results, ulong hash64, ulong threshold64, ReadOnlySpan<ulong> hash256)
    {
        using var _ = _lock.ReaderLock();

        return _hashCollection.NearestNeighbors(results, hash64, threshold64, hash256);
    }

    public Task SerializeAsync(Stream stream)
    {
        using var _ = _lock.ReaderLock();

        return _hashCollection.SerializeAsync(stream);
    }

    public Task DeserializeAsync(Stream stream)
    {
        using var _ = _lock.WriterLock();

        return _hashCollection.DeserializeAsync(stream);
    }
}
