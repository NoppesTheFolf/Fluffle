using Noppes.Fluffle.Main.Communication;
using Noppes.Fluffle.Utils;
using System.Collections.Generic;

namespace Noppes.Fluffle.Sync;

public abstract class SyncProducer : Producer<ICollection<PutContentModel>>
{
}

public abstract class SyncConsumer : Consumer<ICollection<PutContentModel>>
{
}
