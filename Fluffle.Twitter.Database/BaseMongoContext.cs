using MongoDB.Driver;
using MongoDB.Driver.Core.Events;
using Serilog;

namespace Noppes.Fluffle.Twitter.Database;

public class BaseMongoContext
{
    protected readonly IMongoDatabase Database;

    private readonly IList<IMongoEventListener> _eventListeners;

    public BaseMongoContext(string connectionString, string databaseName)
    {
        _eventListeners = new List<IMongoEventListener>();

        var settings = MongoClientSettings.FromConnectionString(connectionString);
        settings.ClusterConfigurator = options =>
        {
            options.Subscribe<CommandStartedEvent>(HandleEvent);
            options.Subscribe<CommandSucceededEvent>(HandleEvent);
            options.Subscribe<CommandFailedEvent>(HandleEvent);
        };

        var mongoClient = new MongoClient(settings);
        Database = mongoClient.GetDatabase(databaseName);
    }

    private void HandleEvent<T>(T e)
    {
        foreach (var listener in _eventListeners)
        {
            Action handleEvent = e switch
            {
                CommandStartedEvent commandStartedEvent => () => listener.Handle(commandStartedEvent),
                CommandSucceededEvent commandSucceededEvent => () => listener.Handle(commandSucceededEvent),
                CommandFailedEvent commandFailedEvent => () => listener.Handle(commandFailedEvent),
                _ => throw new InvalidOperationException()

            };

            try
            {
                handleEvent();
            }
            catch (Exception exception)
            {
                Log.Error(exception, "An error occurred while while handling a {eventType} using handler {eventHandlerType}", e.GetType(), listener.GetType());
            }
        }
    }

    public void AddEventListener(IMongoEventListener listener)
    {
        _eventListeners.Add(listener);
    }
}
