namespace Fluffle.Feeder.Bluesky.JetstreamProcessor.EventHandlers;

public interface IBlueskyEventHandler
{
    Task RunAsync();
}
