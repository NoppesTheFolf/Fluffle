using System.Runtime.Intrinsics.X86;

namespace Noppes.Fluffle.Search.Business.Similarity;

internal class HashCollection
{
    private const int ArrayResizeStepSize = 1_000_000;

    private int[] _ids;
    private ulong[] _hash64s;
    private ulong[] _hash256s;
    private int _size;

    public HashCollection(int initialSize = 0)
    {
        _ids = new int[initialSize];
        _hash64s = new ulong[initialSize];
        _hash256s = new ulong[initialSize * 4];
    }

    public void Add(int id, ulong hash64, ReadOnlySpan<ulong> hash256)
    {
        if (_ids.Length == _size)
        {
            Array.Resize(ref _ids, _ids.Length + ArrayResizeStepSize);
            Array.Resize(ref _hash64s, _hash64s.Length + ArrayResizeStepSize);
            Array.Resize(ref _hash256s, _hash256s.Length + ArrayResizeStepSize * 4);

            GC.Collect();
        }

        _ids[_size] = id;
        _hash64s[_size] = hash64;
        var offset256 = _size * 4;
        _hash256s[offset256] = hash256[0];
        _hash256s[offset256 + 1] = hash256[1];
        _hash256s[offset256 + 2] = hash256[2];
        _hash256s[offset256 + 3] = hash256[3];

        _size++;
    }

    public NearestNeighborsResults NearestNeighbors(ulong hash64, ulong threshold64, ReadOnlySpan<ulong> hash256, int k)
    {
        var results = new List<NearestNeighborsResult>();

        for (var i = 0; i < _size; i++)
        {
            var mismatchCount64 = Popcnt.X64.PopCount(_hash64s[i] ^ hash64);

            if (mismatchCount64 > threshold64)
                continue;

            var offset256 = i * 4;
            var mismatchCount256 = Popcnt.X64.PopCount(_hash256s[offset256] ^ hash256[0]);
            mismatchCount256 += Popcnt.X64.PopCount(_hash256s[offset256 + 1] ^ hash256[1]);
            mismatchCount256 += Popcnt.X64.PopCount(_hash256s[offset256 + 2] ^ hash256[2]);
            mismatchCount256 += Popcnt.X64.PopCount(_hash256s[offset256 + 3] ^ hash256[3]);

            results.Add(new NearestNeighborsResult(_ids[i], (int)mismatchCount256));
        }

        var count256 = results.Count;
        results = results
            .OrderBy(x => x.MismatchCount)
            .ThenBy(x => x.Id)
            .Take(k)
            .ToList();

        return new NearestNeighborsResults(_size, count256, results);
    }
}
