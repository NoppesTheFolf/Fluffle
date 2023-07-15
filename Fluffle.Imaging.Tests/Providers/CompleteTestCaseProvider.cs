using Noppes.Fluffle.Imaging.Tests.Models;

namespace Noppes.Fluffle.Imaging.Tests;

internal class CompleteTestCaseProvider : ITestCaseProvider
{
    private readonly PreconvertedTestCaseProvider _preconvertedTestCaseProvider;
    private readonly ThumbnailTestCaseProvider _thumbnailTestCaseProvider;

    public CompleteTestCaseProvider(PreconvertedTestCaseProvider preconvertedTestCaseProvider, ThumbnailTestCaseProvider thumbnailTestCaseProvider)
    {
        _preconvertedTestCaseProvider = preconvertedTestCaseProvider;
        _thumbnailTestCaseProvider = thumbnailTestCaseProvider;
    }

    public IEnumerable<TestCase> Provide()
    {
        return _preconvertedTestCaseProvider.Provide().Concat(_thumbnailTestCaseProvider.Provide());
    }
}