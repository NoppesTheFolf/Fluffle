namespace Noppes.Fluffle.Queue;

public interface IQueueProvider
{
    IQueue<T> Get<T>(string name);
}