using Noppes.Fluffle.Imaging.Tests.Models;
using Noppes.Fluffle.Constants;
using Noppes.Fluffle.Thumbnail;

namespace Noppes.Fluffle.Imaging.Tests;

internal class ThumbnailTestCaseProvider : ITestCaseProvider
{
    private const int TargetSize = 450;
    private const int Quality = 95;
    
    private readonly FluffleThumbnail _thumbnailer;
    private readonly TestCaseHashesProvider _hashesProvider;
    private readonly Logger _logger;

    public ThumbnailTestCaseProvider(FluffleThumbnail thumbnailer, TestCaseHashesProvider hashesProvider, Logger logger)
    {
        _thumbnailer = thumbnailer;
        _hashesProvider = hashesProvider;
        _logger = logger;
    }

    public IEnumerable<TestCase> Provide()
    {
        // Delete any thumbnails that might already exist
        var existingFiles = Directory.GetFiles(Directories.ThumbnailDestDir);
        foreach (var existingFile in existingFiles)
            File.Delete(existingFile);
        
        foreach (var sourceImage in Directory.GetFiles(Directories.SourceImagesDir))
        foreach (var imageFormat in Enum.GetValues<ImageFormatConstant>())
        {
            var sourceImageFileName = Path.GetFileName(sourceImage);
            var thumbnailDestLocation = Path.Join(Directories.ThumbnailDestDir, $"{sourceImageFileName}.{imageFormat.GetFileExtension()}");
            
            _logger.Write($"Creating {imageFormat} thumbnail with size {TargetSize} for {sourceImageFileName}");
            _thumbnailer.Generate(sourceImage, thumbnailDestLocation, TargetSize, imageFormat, Quality);

            var testCase = new TestCase
            {
                AllowedDeviationAsPercentage = 0.05, // The main purpose of these test cases is to ensure thumbnailing functionality works as expected
                Description = $"Create {imageFormat} thumbnail with size {TargetSize} from {sourceImageFileName}",
                ExpectedResultImage = sourceImage,
                ExpectedResult = _hashesProvider.Provide(sourceImage),
                ActualResultImage = thumbnailDestLocation,
                ActualResult = _hashesProvider.Provide(thumbnailDestLocation)
            };

            yield return testCase;
        }
    }
}
