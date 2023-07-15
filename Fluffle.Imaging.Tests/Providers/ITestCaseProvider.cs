using Noppes.Fluffle.Imaging.Tests.Models;

namespace Noppes.Fluffle.Imaging.Tests;

/// <summary>
///     Provides test cases with the intent to ensure one of Fluffle's core component is working correctly: hashing images.
/// </summary>
internal interface ITestCaseProvider
{
    IEnumerable<TestCase> Provide();
}