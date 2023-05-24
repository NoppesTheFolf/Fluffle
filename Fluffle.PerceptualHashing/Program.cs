using Serilog;

namespace Noppes.Fluffle.PerceptualHashing;

internal class Program
{
    private static void Main(string[] args)
    {
        var fluffleHash = new FluffleHash();

        var creator = new FluffleHashSelfTestCreator(fluffleHash);
        creator.Run();

        var runner = new FluffleHashSelfTestRunner(fluffleHash, Log.Information);
        runner.Run();
    }
}
