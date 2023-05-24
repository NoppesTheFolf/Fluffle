using System;

namespace Noppes.Fluffle.Bot.Database;

public class MongoReverseSearchRequestHistory
{
    public long Id { get; set; }

    public DateTime[] Values { get; set; }

    public int Increment { get; set; }
}
