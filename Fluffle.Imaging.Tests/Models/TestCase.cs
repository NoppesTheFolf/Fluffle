namespace Noppes.Fluffle.Imaging.Tests.Models;

internal class TestCase
{
    public string Description { get; set; } = null!;

    public int? AllowedDeviationInBits { get; set; }

    public double? AllowedDeviationAsPercentage { get; set; }

    /// <summary>
    /// The image used to calculate the expected result.
    /// </summary>
    public string ExpectedResultImage { get; set; } = null!;

    public TestCaseHashes ExpectedResult { get; set; } = null!;

    /// <summary>
    /// The image used to calculate the actual result.
    /// </summary>
    public string? ActualResultImage { get; set; }

    public TestCaseHashes ActualResult { get; set; } = null!;
}