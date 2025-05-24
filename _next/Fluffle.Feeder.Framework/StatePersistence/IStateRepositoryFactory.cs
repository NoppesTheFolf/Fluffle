namespace Fluffle.Feeder.Framework.StatePersistence;

public interface IStateRepositoryFactory
{
    IStateRepository<T> Create<T>(string id);
}
