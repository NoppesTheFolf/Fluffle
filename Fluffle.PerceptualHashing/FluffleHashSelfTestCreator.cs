using Nitranium.PerceptualHashing;
using System.IO;
using System.Text.Json;

namespace Noppes.Fluffle.PerceptualHashing;

internal class FluffleHashSelfTestCreator
{
    private const string Location = "FluffleHashTestsCreate";

    private readonly FluffleHash _fluffleHash;

    public FluffleHashSelfTestCreator(FluffleHash fluffleHash)
    {
        _fluffleHash = fluffleHash;
    }

    public void Run()
    {
        foreach (var location in Directory.GetFiles(Location, "*"))
        {
            if (Path.GetExtension(location) == ".json")
                continue;

            var results = new FluffleHashSelfTestResults();
            var hash = _fluffleHash.Create(128);
            using var hasher = hash.For(location);

            results.PhashRed1024 = hasher.ComputeHash(Channel.Red);
            results.PhashGreen1024 = hasher.ComputeHash(Channel.Green);
            results.PhashBlue1024 = hasher.ComputeHash(Channel.Blue);
            results.PhashAverage1024 = hasher.ComputeHash(Channel.Average);

            hash.Size = 32;
            results.PhashRed256 = hasher.ComputeHash(Channel.Red);
            results.PhashGreen256 = hasher.ComputeHash(Channel.Green);
            results.PhashBlue256 = hasher.ComputeHash(Channel.Blue);
            results.PhashAverage256 = hasher.ComputeHash(Channel.Average);

            hash.Size = 8;
            results.PhashRed64 = hasher.ComputeHash(Channel.Red);
            results.PhashGreen64 = hasher.ComputeHash(Channel.Green);
            results.PhashBlue64 = hasher.ComputeHash(Channel.Blue);
            results.PhashAverage64 = hasher.ComputeHash(Channel.Average);

            var resultsJson = JsonSerializer.Serialize(results, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });
            File.WriteAllText(location + ".json", resultsJson);
        }
    }
}
