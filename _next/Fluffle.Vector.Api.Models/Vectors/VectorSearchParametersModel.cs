﻿namespace Fluffle.Vector.Api.Models.Vectors;

public class VectorSearchParametersModel
{
    public required float[] Query { get; set; }

    public required int Limit { get; set; }
}
