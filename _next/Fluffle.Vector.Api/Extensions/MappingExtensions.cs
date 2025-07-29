using Fluffle.Vector.Api.Models.Items;
using Fluffle.Vector.Core.Domain.Items;
using System.Text.Json;

namespace Fluffle.Vector.Api.Extensions;

internal static class MappingExtensions
{
    public static ItemModel ToModel(this Item item)
    {
        return new ItemModel
        {
            ItemId = item.ItemId,
            GroupId = item.GroupId,
            Images = item.Images.Select(x => new ImageModel
            {
                Width = x.Width,
                Height = x.Height,
                Url = x.Url
            }).ToList(),
            Thumbnail = item.Thumbnail == null ? null : new ThumbnailModel
            {
                Width = item.Thumbnail.Width,
                Height = item.Thumbnail.Height,
                CenterX = item.Thumbnail.CenterX,
                CenterY = item.Thumbnail.CenterY,
                Url = item.Thumbnail.Url
            },
            Properties = JsonSerializer.SerializeToNode(item.Properties) ??
                         throw new InvalidOperationException("Item properties should never serialize to null.")
        };
    }
}
