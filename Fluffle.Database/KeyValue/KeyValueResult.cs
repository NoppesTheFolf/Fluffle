namespace Noppes.Fluffle.Database.KeyValue;

public class KeyValueResult<T>
{
    public T Value { get; set; }

    public KeyValueResult(T value)
    {
        Value = value;
    }
}
