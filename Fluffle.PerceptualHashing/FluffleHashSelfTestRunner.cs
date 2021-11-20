﻿using Nitranium.PerceptualHashing;
using System;
using System.IO;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text.Json;

namespace Noppes.Fluffle.PerceptualHashing
{
    public class FluffleHashSelfTestRunner
    {
        private const string TestsLocation = "FluffleHashTests";
        private const int AllowedMismatchCount = 6;

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

                var hash = _fluffleHash.Create(128);
                using var hasher = hash.For(imageFile);
                var name = Path.GetFileNameWithoutExtension(imageFile);

                Compare(name, expected.PhashRed1024, hasher, Channel.Red);
                Compare(name, expected.PhashGreen1024, hasher, Channel.Green);
                Compare(name, expected.PhashBlue1024, hasher, Channel.Blue);
                Compare(name, expected.PhashAverage1024, hasher, Channel.Average);

                hash.Size = 32;
                Compare(name, expected.PhashRed256, hasher, Channel.Red);
                Compare(name, expected.PhashGreen256, hasher, Channel.Green);
                Compare(name, expected.PhashBlue256, hasher, Channel.Blue);
                Compare(name, expected.PhashAverage256, hasher, Channel.Average);

                hash.Size = 8;
                Compare(name, expected.PhashRed64, hasher, Channel.Red);
                Compare(name, expected.PhashGreen64, hasher, Channel.Green);
                Compare(name, expected.PhashBlue64, hasher, Channel.Blue);
                Compare(name, expected.PhashAverage64, hasher, Channel.Average);
            }

            Log("Self test ran successfully.");
        }

        private void Compare(string name, ReadOnlySpan<byte> expected, PerceptualHashImage image, Channel channel)
        {
            var actual = image.ComputeHash(channel);
            ulong mismatchCount = 0;
            foreach (var (b1, b2) in FluffleHash.ToInt64(actual).Zip(FluffleHash.ToInt64(expected.ToArray())))
                mismatchCount += Popcnt.X64.PopCount(b1 ^ b2);

            var lengthInBits = 8 * sizeof(byte) * actual.Length;
            var percentageWrong = (int)mismatchCount / (double)lengthInBits;
            var percentageWrongAllowed = AllowedMismatchCount / (double)lengthInBits;

            Log($"Test {name} {channel}@{expected.Length} | mismatch: {percentageWrong}, allowed: {percentageWrongAllowed}");

            if (percentageWrong > percentageWrongAllowed)
                throw new InvalidOperationException($"Hashing did not produce expected result.");
        }
    }
}
