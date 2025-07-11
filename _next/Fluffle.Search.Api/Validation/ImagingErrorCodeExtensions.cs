using Fluffle.Imaging.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace Fluffle.Search.Api.Validation;

public static class ImagingErrorCodeExtensions
{
    public static ObjectResult AsResult(this ImagingErrorCode? code)
    {
        if (code == ImagingErrorCode.UnsupportedImage)
        {
            return Error.Create(
                statusCode: 415,
                code: "unsupportedFileType",
                message: "The type of the submitted file isn't supported. Only JPEG, PNG, WebP and GIF are. " +
                         "If you're getting this error even though the image seems to be valid, it might be that the extension is not representative of the encoding."
            );
        }

        if (code == ImagingErrorCode.ImageAreaTooLarge)
        {
            return Error.Create(
                statusCode: 400,
                code: "areaTooLarge",
                message: "The submitted image has an area (width * height) greater than the maximum allowed area of 16 megapixels."
            );
        }

        return Error.Create(
            statusCode: 422,
            code: "corruptFile",
            message: "The submitted file could not be read by Fluffle. This likely means it's corrupt."
        );
    }
}
