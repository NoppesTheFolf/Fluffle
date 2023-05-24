using System;

namespace Noppes.Fluffle.Bot.Database;

public class MongoMediaGroup
{
    public string Id { get; set; }

    public string FluffleId { get; set; }

    public bool HasResults { get; set; }

    public DateTime When { get; set; }
}
