using Noppes.Fluffle.Main.Client;
using Noppes.Fluffle.Main.Communication;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Sync;

public abstract class SyncState
{
    [JsonIgnore]
    public int Version { get; set; }
}

public class SyncStateService<T> where T : SyncState, new()
{
    public T State { get; private set; }

    private readonly PlatformModel _platform;
    private readonly FluffleClient _fluffleClient;

    public SyncStateService(PlatformModel platform, FluffleClient fluffleClient)
    {
        _platform = platform;
        _fluffleClient = fluffleClient;
    }

    public async Task<T> InitializeAsync(Func<T, Task> initializeAsync)
    {
        var remoteState = await GetAsync();

        if (remoteState != null)
        {
            State = remoteState;
            return State;
        }

        State = new T();
        await initializeAsync(State);
        await SyncAsync();
        return State;
    }

    private async Task<T> GetAsync()
    {
        var model = await _fluffleClient.GetSyncStateAsync(_platform.NormalizedName);

        if (model == null)
            return null;

        var syncState = JsonSerializer.Deserialize<T>(model.Document);
        syncState.Version = model.Version;

        return syncState;
    }

    public Task SyncAsync()
    {
        return _fluffleClient.PutSyncStateAsync(_platform.NormalizedName, new SyncStateModel
        {
            Document = JsonSerializer.Serialize(State),
            Version = State.Version
        });
    }
}
