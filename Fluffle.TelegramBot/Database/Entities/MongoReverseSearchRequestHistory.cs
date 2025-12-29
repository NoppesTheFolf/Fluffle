using System;

namespace Fluffle.TelegramBot.Database.Entities;

public class MongoReverseSearchRequestHistory
{
    public long Id { get; set; }

    public DateTime[] Values { get; set; }

    public int Increment { get; set; }
}
