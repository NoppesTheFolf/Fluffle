namespace Fluffle.Vector.Api.Models.Vectors;

public class VectorSearchModel
{
    public required float[] Query { get; set; }

    public required int Limit { get; set; }
}
