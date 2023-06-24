using MongoDB.Driver.Core.Events;

namespace Noppes.Fluffle.Twitter.Database;

public interface IMongoEventListener
{
    public void Handle(CommandStartedEvent commandStartedEvent);

    public void Handle(CommandSucceededEvent commandSucceededEvent);

    public void Handle(CommandFailedEvent commandFailedEvent);
}
