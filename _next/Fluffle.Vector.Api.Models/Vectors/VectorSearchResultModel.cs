﻿using System.Text.Json.Nodes;

namespace Fluffle.Vector.Api.Models.Vectors;

public class VectorSearchResultModel
{
    public required string ItemId { get; set; }

    public required float Distance { get; set; }

    public required JsonNode Properties { get; set; }
}
