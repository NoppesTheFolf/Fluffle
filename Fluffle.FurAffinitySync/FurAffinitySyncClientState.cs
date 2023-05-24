using Nito.AsyncEx;
using Noppes.Fluffle.Sync;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Noppes.Fluffle.FurAffinitySync;

public class FurAffinitySyncClientState : SyncState
{
    public int ArchiveStartId { get; set; }

    public int ArchiveEndId { get; set; }

    public ICollection<string> ProcessedArtists { get; set; }

    private readonly AsyncLock _lock;

    public FurAffinitySyncClientState()
    {
        _lock = new AsyncLock();

        ProcessedArtists = new List<string>();
    }

    public T Acquire<T>(Func<FurAffinitySyncClientState, T> use)
    {
        using var _ = _lock.Lock();

        return use(this);
    }

    public async Task<T> AcquireAsync<T>(Func<FurAffinitySyncClientState, Task<T>> useAsync)
    {
        using var _ = await _lock.LockAsync();

        return await useAsync(this);
    }
}
