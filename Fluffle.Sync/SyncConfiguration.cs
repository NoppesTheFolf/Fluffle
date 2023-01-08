using Noppes.Fluffle.Constants;
using Noppes.Fluffle.Main.Communication;

namespace Noppes.Fluffle.Sync;

public class SyncConfiguration
{
    public PlatformModel Platform { get; }

    public SyncTypeConstant SyncType { get; }

    public SyncConfiguration(PlatformModel platform, SyncTypeConstant syncType)
    {
        Platform = platform;
        SyncType = syncType;
    }
}
