﻿using System.ComponentModel.DataAnnotations;

namespace Fluffle.Ingestion.Mongo;

internal class MongoOptions
{
    public const string Mongo = "Mongo";

    [Required]
    public required string ConnectionString { get; set; }

    [Required]
    public required string DatabaseName { get; set; }
}
