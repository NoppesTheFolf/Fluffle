using System.Text.Json;
using Noppes.Fluffle.Imaging.Tests.Models;

namespace Noppes.Fluffle.Imaging.Tests;

internal class PreconvertedTestCaseProvider : ITestCaseProvider
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly TestCaseHashesProvider _hashesProvider;

    public PreconvertedTestCaseProvider(TestCaseHashesProvider hashesProvider)
    {
        _hashesProvider = hashesProvider;
    }

    public IEnumerable<TestCase> Provide()
    {
        foreach (var imageLocation in Directory.GetFiles(Directories.PreconvertedImagesDir))
        {
            var imageFilename = Path.GetFileName(imageLocation);
            var expectedResultsLocation = Path.Join(Directories.PreconvertedExpectedDir, imageFilename + ".json");
            var expectedResults = JsonSerializer.Deserialize<TestCaseHashes>(File.ReadAllText(expectedResultsLocation), SerializerOptions)!;
            var testCase = new TestCase
            {
                Description = $"Preconverted and prehashed image {imageFilename}",
                AllowedDeviationInBits = 6, // Be very strict about the preconverted images being the same, this keeps Fluffle consistent over time
                ExpectedResultImage = imageLocation,
                ExpectedResult = expectedResults,
                ActualResultImage = imageLocation,
                ActualResult = _hashesProvider.Provide(imageLocation)
            };

            yield return testCase;
        }
    }
}
