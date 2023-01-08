using CommandLine;
using Noppes.Fluffle.Constants;

namespace Noppes.Fluffle.Sync;

public class SyncCommandLineOptions
{
    [Option('s', "sync-type", Required = true)]
    public SyncTypeConstant SyncType { get; set; }
}
