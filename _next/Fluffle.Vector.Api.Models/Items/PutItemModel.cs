namespace Fluffle.Vector.Api.Models.Items;

public class PutItemModel
{
    public required ICollection<ImageModel> Images { get; set; }

    public IDictionary<string, object?>? Properties { get; set; }
}
