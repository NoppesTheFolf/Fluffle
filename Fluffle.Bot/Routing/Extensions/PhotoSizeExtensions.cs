using System.Collections.Generic;
using System.Linq;
using Telegram.Bot.Types;

namespace Noppes.Fluffle.Bot.Routing;

public static class PhotoSizeExtensions
{
    public static PhotoSize Largest(this IEnumerable<PhotoSize> photoSizes)
    {
        return photoSizes.OrderByDescending(x => x.Area()).First();
    }

    public static int Area(this PhotoSize photoSize) => photoSize.Width * photoSize.Height;
}
