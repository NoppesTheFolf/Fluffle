using Fluffle.Imaging.Api.Models;

namespace Fluffle.Imaging.Api.Validation;

public static class ImagingExceptions
{
    public static ImagingException EmptyBody() => new(
        code: ImagingErrorCode.EmptyBody,
        message: "No body was provided in the request."
    );

    public static ImagingException ImageAreaTooLarge(int actualArea, int allowedArea) => new(
        code: ImagingErrorCode.ImageAreaTooLarge,
        message: $"The image has an area of {actualArea}. This is more than the limit of {allowedArea}."
    );

    public static ImagingException FileSizeTooLarge(long actualFileSize, long allowedFileSize) => new(
        code: ImagingErrorCode.FileSizeTooLarge,
        message: $"The file has a size of {actualFileSize}. This is more than the limit of {allowedFileSize}."
    );

    public static ImagingException UnsupportedImage() => new(
        code: ImagingErrorCode.UnsupportedImage,
        message: "The file is not in one of the supported formats (JPEG, PNG, GIF, WebP)."
    );

    public static ImagingException QualityOutOfRange(int minQuality, int maxQuality) => new(
        code: ImagingErrorCode.QualityOutOfRange,
        message: $"The quality parameter must be between {minQuality} and {maxQuality}."
    );

    public static ImagingException SizeOutOfRange(int minSize) => new(
        code: ImagingErrorCode.SizeOutOfRange,
        message: $"The size parameter must greater than or equal to {minSize}."
    );
}
