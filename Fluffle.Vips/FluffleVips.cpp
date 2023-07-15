#include "FluffleVips.h"

#include <vips/vips8>
#include <iostream>
#include <string.h>
#include <tr1/functional>

using namespace std;
using namespace vips;

bool VipsInit()
{
    return !VIPS_INIT("");
}

/**
 * Flattens the given image. That is, removing transparency and replacing it with a white background.
 * Returns the origin image if it doesn't have a alpha channel.
 */
VImage flatten(VImage in)
{
    if (!in.has_alpha())
    {
        return in;
    }

    return in.flatten(VImage::option()->set("background", 255));
}

/**
 * Converts the given image to the sRGB colorspace. Returns the original image if
 * it's already using the sRGB colorspace.
 */
VImage srgb(VImage in)
{
    if (in.interpretation() == VipsInterpretation::VIPS_INTERPRETATION_sRGB)
    {
        return in;
    }

    return in.colourspace(VipsInterpretation::VIPS_INTERPRETATION_sRGB);
}

VImage thumbnail(const char *srcLocation, int targetWidth, int targetHeight)
{
    if (targetHeight < 1 || targetWidth < 1)
    {
        throw invalid_argument("Neither width nor height can be less than 1.");
    }

    VImage in = VImage::new_from_file(srcLocation, VImage::option()->set("access", VIPS_ACCESS_SEQUENTIAL));
    in = srgb(in);
    in = flatten(in);

    in = in.thumbnail_image(targetWidth, VImage::option()
                                             ->set("height", targetHeight)
                                             ->set("size", VipsSize::VIPS_SIZE_FORCE)
                                             ->set("crop", VipsInteresting::VIPS_INTERESTING_NONE));

    return in;
}

ThumbnailResult Thumbnail(const char *srcLocation, int width, int height, std::tr1::function<void(VImage)> save)
{
    ThumbnailResult result;

    try
    {
        VImage in = thumbnail(srcLocation, width, height);

        save(in);

        result.Width = in.width();
        result.Height = in.height();
    }
    catch (const exception &exception)
    {
        result.Error = strdup(exception.what());

        return result;
    }

    result.Error = strdup("");
    return result;
}

ThumbnailResult ThumbnailWebP(const char *srcLocation, const char *destLocation, int width, int height, int quality)
{
    auto save = [quality, destLocation](VImage image) {
        image.webpsave(destLocation, VImage::option()
                                         ->set("Q", quality)
                                         ->set("effort", 6)
                                         ->set("strip", true));
    };

    return Thumbnail(srcLocation, width, height, save);
}

ThumbnailResult ThumbnailJpeg(const char *srcLocation, const char *destLocation, int width, int height, int quality)
{
    auto save = [quality, destLocation](VImage image) {
        image.jpegsave(destLocation, VImage::option()
                                         ->set("Q", quality)
                                         ->set("optimize-coding", true)
                                         ->set("interlace", true)
                                         ->set("subsample-mode", VipsForeignJpegSubsample::VIPS_FOREIGN_JPEG_SUBSAMPLE_ON)
                                         ->set("trellis-quant", true)
                                         ->set("overshoot-deringing", true)
                                         ->set("optimize-scans", true)
                                         ->set("strip", true));
    };

    return Thumbnail(srcLocation, width, height, save);
}

ThumbnailResult ThumbnailAvif(const char *srcLocation, const char *destLocation, int width, int height, int quality)
{
    auto save = [quality, destLocation](VImage image) {
        image.heifsave(destLocation, VImage::option()
                                         ->set("Q", quality)
                                         ->set("compression", VipsForeignHeifCompression::VIPS_FOREIGN_HEIF_COMPRESSION_AV1)
                                         ->set("effort", 9)
                                         ->set("strip", true));
    };

    return Thumbnail(srcLocation, width, height, save);
}

ThumbnailResult ThumbnailPpm(const char *srcLocation, const char *destLocation, int width, int height)
{
    auto save = [destLocation](VImage image) {
        image.ppmsave(destLocation, VImage::option()->set("strip", true));
    };

    return Thumbnail(srcLocation, width, height, save);
}

ImageDimensions GetDimensions(const char *srcLocation)
{
    ImageDimensions result;

    try
    {
        VImage in = VImage::new_from_file(srcLocation);

        result.Width = in.width();
        result.Height = in.height();
    }
    catch (const exception &exception)
    {
        result.Error = strdup(exception.what());

        return result;
    }

    result.Error = strdup("");
    return result;
}

CenterResult Center(const char *location)
{
    CenterResult result;

    try
    {
        VImage in = VImage::new_from_file(location);
        auto target = in.width() < in.height() ? in.width() : in.height();

        VImage out = in.smartcrop(target, target, VImage::option()->set("interesting", VipsInteresting::VIPS_INTERESTING_ATTENTION));

        // Turn the negative values produced by [x,y]offset() positive
        int xoffset = abs(out.xoffset());
        int yoffset = abs(out.yoffset());

        int xleft = in.width() - target;
        int yleft = in.height() - target;

        result.X = (int)floor(xleft == 0 ? 0 : (double)xoffset / xleft * 100);
        result.Y = (int)floor(yleft == 0 ? 0 : (double)yoffset / yleft * 100);
    }
    catch (const exception &exception)
    {
        result.Error = strdup(exception.what());

        return result;
    }

    result.Error = strdup("");
    return result;
}
