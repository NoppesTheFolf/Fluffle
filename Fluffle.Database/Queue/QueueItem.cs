namespace Noppes.Fluffle.Database.Queue;

public class QueueItem<T>
{
    public long Id { get; set; }

    public T Data { get; set; }
}
