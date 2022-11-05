namespace Noppes.Fluffle.Queue;

public abstract class QueueItem<T>
{
    public T Value { get; set; }

    protected QueueItem(T value)
    {
        Value = value;
    }

    public abstract Task AcknowledgeAsync();
}