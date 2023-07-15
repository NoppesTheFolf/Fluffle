using Noppes.Fluffle.Imaging.Tests.Models;
using Nitranium.PerceptualHashing;
using Noppes.Fluffle.PerceptualHashing;

namespace Noppes.Fluffle.Imaging.Tests;

internal class TestCaseHashesProvider
{
    private readonly FluffleHash _fluffleHash;

    public TestCaseHashesProvider(FluffleHash fluffleHash)
    {
        _fluffleHash = fluffleHash;
    }

    public TestCaseHashes Provide(string imageLocation)
    {
        var hashes = new TestCaseHashes();
        var hash = _fluffleHash.Create(128);
        using var hasher = hash.For(imageLocation);

        hashes.PhashRed1024 = hasher.ComputeHash(Channel.Red);
        hashes.PhashGreen1024 = hasher.ComputeHash(Channel.Green);
        hashes.PhashBlue1024 = hasher.ComputeHash(Channel.Blue);
        hashes.PhashAverage1024 = hasher.ComputeHash(Channel.Average);

        hash.Size = 32;
        hashes.PhashRed256 = hasher.ComputeHash(Channel.Red);
        hashes.PhashGreen256 = hasher.ComputeHash(Channel.Green);
        hashes.PhashBlue256 = hasher.ComputeHash(Channel.Blue);
        hashes.PhashAverage256 = hasher.ComputeHash(Channel.Average);

        hash.Size = 8;
        hashes.PhashRed64 = hasher.ComputeHash(Channel.Red);
        hashes.PhashGreen64 = hasher.ComputeHash(Channel.Green);
        hashes.PhashBlue64 = hasher.ComputeHash(Channel.Blue);
        hashes.PhashAverage64 = hasher.ComputeHash(Channel.Average);

        return hashes;
    }
}