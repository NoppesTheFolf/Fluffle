using System;
using System.Collections.Generic;
using System.Linq;

namespace Noppes.Fluffle.Utils
{
    public static class ImageSizeHelper
    {
        public static IEnumerable<T> OrderByDownloadPreference<T>(IEnumerable<T> files, Func<T, int> getWidth, Func<T, int> getHeight, int target)
        {
            var imagesByArea = files
                .OrderBy(s => getWidth(s) * getHeight(s))
                .ToList();

            var preferredImages = imagesByArea
                .Where(s => getWidth(s) >= target && getHeight(s) >= target)
                .ToList();

            var leftOverImages = imagesByArea
                .Except(preferredImages)
                .OrderByDescending(s => getWidth(s) * getHeight(s));

            return preferredImages.Concat(leftOverImages);
        }
    }
}
