using Noppes.Fluffle.Utils;
using System.Runtime.Intrinsics.X86;

namespace Noppes.Fluffle.Search.Business.Similarity;

internal class HashCollection : IHashCollection
{
    private readonly int _resizeStepSize;

    private int[] _ids;
    // ReSharper disable once InconsistentNaming
    private ulong[] _hash64s;
    // ReSharper disable once InconsistentNaming
    private ulong[] _hash256s;
    private int _size;

    public HashCollection(int initialSize, int resizeStepSize)
    {
        _resizeStepSize = resizeStepSize;

        _ids = new int[initialSize];
        _hash64s = new ulong[initialSize];
        _hash256s = new ulong[initialSize * 4];
    }

    public void Add(int id, ulong hash64, ReadOnlySpan<ulong> hash256)
    {
        if (_ids.Length == _size)
        {
            Array.Resize(ref _ids, _ids.Length + _resizeStepSize);
            Array.Resize(ref _hash64s, _hash64s.Length + _resizeStepSize);
            Array.Resize(ref _hash256s, _hash256s.Length + _resizeStepSize * 4);

            GC.Collect();
        }

        _ids[_size] = id;
        _hash64s[_size] = hash64;
        hash256.CopyTo(Get256Span(_size));

        _size++;
    }

    public bool TryRemove(int id)
    {
        var idx = _ids.AsSpan(0, _size).IndexOf(id);
        if (idx == -1)
            return false;

        // The code below moves the last hash to the position of the hash being removed (overwriting
        // it), then removing the last hash in the list
        var lastIdx = _size - 1;
        _ids[idx] = _ids[lastIdx];
        _hash64s[idx] = _hash64s[lastIdx];
        Get256Span(lastIdx).CopyTo(Get256Span(idx));

        _ids[lastIdx] = default;
        _hash64s[lastIdx] = default;
        Get256Span(lastIdx).Clear();

        _size--;

        return true;
    }

    private Span<ulong> Get256Span(int idx) => _hash256s.AsSpan(idx * 4, 4);

    public NearestNeighborsStats NearestNeighbors(TopNList<NearestNeighborsResult> results, ulong hash64, ulong threshold64, ReadOnlySpan<ulong> hash256)
    {
        var count256 = 0;
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
            count256++;
        }

        return new NearestNeighborsStats(_size, count256);
    }

    public async Task SerializeAsync(Stream stream)
    {
        await stream.WriteInt32LittleEndianAsync(_size);
        await stream.WriteInt32LittleEndianAsync(_ids.AsMemory(0, _size));
        await stream.WriteUInt64LittleEndianAsync(_hash64s.AsMemory(0, _size));
        await stream.WriteUInt64LittleEndianAsync(_hash256s.AsMemory(0, _size * 4));
    }

    public async Task DeserializeAsync(Stream stream)
    {
        _size = await stream.ReadInt32LittleEndianAsync();

        _ids = new int[_size];
        await stream.ReadInt32LittleEndianAsync(_ids.AsMemory(0, _ids.Length));

        _hash64s = new ulong[_size];
        await stream.ReadUInt64LittleEndianAsync(_hash64s.AsMemory(0, _hash64s.Length));

        _hash256s = new ulong[_size * 4];
        await stream.ReadUInt64LittleEndianAsync(_hash256s.AsMemory(0, _hash256s.Length));
    }
}
