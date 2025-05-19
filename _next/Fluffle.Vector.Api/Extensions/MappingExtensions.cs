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
            Images = item.Images.Select(x => new ImageModel
            {
                Width = x.Width,
                Height = x.Height,
                Url = x.Url
            }).ToList(),
            Properties = JsonSerializer.SerializeToNode(item.Properties) ??
                         throw new InvalidOperationException("Item properties should never serialize to null.")
        };
    }
}
