namespace Fluffle.Feeder.Framework.StatePersistence;

public interface IStateRepository<T>
{
    Task PutAsync(T state);

    Task<T?> GetAsync();
}
