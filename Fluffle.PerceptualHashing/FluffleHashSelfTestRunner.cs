using Nitranium.PerceptualHashing;
using System;
using System.IO;
using System.Text.Json;

namespace Noppes.Fluffle.PerceptualHashing
{
    public class FluffleHashSelfTestRunner
    {
        private const string TestsLocation = "FluffleHashTests";

        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public Action<string> Log { get; set; }

        private readonly FluffleHash _fluffleHash;

        public FluffleHashSelfTestRunner(FluffleHash fluffleHash)
        {
            _fluffleHash = fluffleHash;
        }

        public void Run()
        {
            Log("Running self test...");

            foreach (var jsonFile in Directory.GetFiles(TestsLocation, "*.json"))
            {
                var imageFile = Path.Combine(Path.GetDirectoryName(jsonFile) ?? string.Empty, Path.GetFileNameWithoutExtension(jsonFile));

                var hashesJson = File.ReadAllText(jsonFile);
                var expected = JsonSerializer.Deserialize<FluffleHashSelfTestResult>(hashesJson, SerializerOptions);

                using var hasher64 = _fluffleHash.Size64.For(imageFile);
                Compare(expected.PhashRed64, hasher64, Channel.Red);
                Compare(expected.PhashGreen64, hasher64, Channel.Green);
                Compare(expected.PhashBlue64, hasher64, Channel.Blue);
                Compare(expected.PhashAverage64, hasher64, Channel.Average);

                using var hasher256 = _fluffleHash.Size256.For(imageFile);
                Compare(expected.PhashRed256, hasher256, Channel.Red);
                Compare(expected.PhashGreen256, hasher256, Channel.Green);
                Compare(expected.PhashBlue256, hasher256, Channel.Blue);
                Compare(expected.PhashAverage256, hasher256, Channel.Average);

                using var hasher1024 = _fluffleHash.Size1024.For(imageFile);
                Compare(expected.PhashRed1024, hasher1024, Channel.Red);
                Compare(expected.PhashGreen1024, hasher1024, Channel.Green);
                Compare(expected.PhashBlue1024, hasher1024, Channel.Blue);
                Compare(expected.PhashAverage1024, hasher1024, Channel.Average);
            }

            Log("Self test ran successfully.");
        }

        private static void Compare(ReadOnlySpan<byte> expected, PerceptualHashImage image, Channel channel)
        {
            var actual = image.ComputeHash(channel);

            if (!expected.SequenceEqual(actual))
                throw new InvalidOperationException($"Hashing did not produce expected result (channel: {channel}, length: {expected.Length})");
        }
    }
}
