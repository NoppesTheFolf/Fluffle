namespace Fluffle.Vector.Api.Models.Items;

public class PutItemVectorModel
{
    public required float[] Value { get; set; }

    public IDictionary<string, object?>? Properties { get; set; }
}
