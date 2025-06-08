using Fluffle.Imaging.Api.Extensions;
using Fluffle.Imaging.Api.Models;
using Microsoft.AspNetCore.Mvc;
using NetVips;
using System.Text.Json;

namespace Fluffle.Imaging.Api.Controllers;

[ApiController]
public class ImagingController : ControllerBase
{
    [HttpPost("/metadata", Name = "GetMetadata")]
    public async Task<IActionResult> GetMetadataAsync()
    {
        using var memoryStream = await HttpContext.Request.Body.ToMemoryStreamAsync();

        using var sourceImage = Image.NewFromStream(memoryStream, access: Enums.Access.Sequential);
        using var rotatedImage = sourceImage.Autorot();

        var (centerX, centerY) = CalculateCenter(rotatedImage);

        var model = new ImageMetadataModel
        {
            Width = rotatedImage.Width,
            Height = rotatedImage.Height,
            Center = new ImageMetadataCenterModel
            {
                X = centerX,
                Y = centerY
            }
        };

        return Ok(model);
    }

    [HttpPost("/thumbnail", Name = "CreateThumbnail")]
    public async Task<IActionResult> CreateThumbnailAsync(int size, int quality, bool calculateCenter)
    {
        using var memoryStream1 = await HttpContext.Request.Body.ToMemoryStreamAsync();

        using var memoryStream2 = new MemoryStream();
        await memoryStream1.CopyToAsync(memoryStream2);

        memoryStream1.Position = 0;
        memoryStream2.Position = 0;

        using var sourceImage = Image.NewFromStream(memoryStream1, access: Enums.Access.Sequential);
        using var rotatedImage = sourceImage.Autorot();

        var (targetWidth, targetHeight) = (rotatedImage.Width, rotatedImage.Height);
        var scalingFactor = size / (double)Math.Min(rotatedImage.Width, rotatedImage.Height);
        if (scalingFactor < 1)
        {
            targetWidth = (int)Math.Round(scalingFactor * rotatedImage.Width);
            targetHeight = (int)Math.Round(scalingFactor * rotatedImage.Height);
        }

        using var thumbnail = Image.ThumbnailStream(
            memoryStream2,
            targetWidth,
            height: targetHeight,
            size: Enums.Size.Force,
            crop: Enums.Interesting.None
        );

        using var colorSpaceThumbnail = thumbnail.Interpretation != Enums.Interpretation.Srgb
            ? thumbnail.Colourspace(Enums.Interpretation.Srgb)
            : thumbnail;

        using var flattenedThumbnail = colorSpaceThumbnail.HasAlpha()
            ? colorSpaceThumbnail.Flatten(background: [255])
            : colorSpaceThumbnail;

        var thumbnailStream = new MemoryStream();
        flattenedThumbnail.JpegsaveStream(
            thumbnailStream,
            q: quality,
            optimizeCoding: true,
            interlace: true,
            subsampleMode: Enums.ForeignSubsample.On,
            keep: Enums.ForeignKeep.None
        );
        thumbnailStream.Position = 0;

        ImageMetadataCenterModel? centerModel = null;
        if (calculateCenter)
        {
            var (centerX, centerY) = CalculateCenter(rotatedImage);
            centerModel = new ImageMetadataCenterModel
            {
                X = centerX,
                Y = centerY
            };
        }

        var thumbnailModelJson = JsonSerializer.Serialize(new ImageMetadataModel
        {
            Width = flattenedThumbnail.Width,
            Height = flattenedThumbnail.Height,
            Center = centerModel
        }, JsonSerializerOptions.Web);
        Response.Headers["Imaging-Metadata"] = thumbnailModelJson;

        return new FileStreamResult(thumbnailStream, "image/jpeg");
    }

    private static (int centerX, int centerY) CalculateCenter(Image image)
    {
        var target = Math.Min(image.Width, image.Height);
        using var smartCroppedImage = image.Smartcrop(
            width: target,
            height: target,
            interesting: Enums.Interesting.Attention
        );

        var xOffset = Math.Abs(smartCroppedImage.Xoffset);
        var yOffset = Math.Abs(smartCroppedImage.Yoffset);

        var xLeft = image.Width - target;
        var yLeft = image.Height - target;

        var centerX = (int)Math.Floor(xLeft == 0 ? 0 : (double)xOffset / xLeft * 100);
        var centerY = (int)Math.Floor(yLeft == 0 ? 0 : (double)yOffset / yLeft * 100);

        return (centerX, centerY);
    }
}
