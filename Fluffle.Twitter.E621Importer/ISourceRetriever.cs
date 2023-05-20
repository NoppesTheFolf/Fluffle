namespace Noppes.Fluffle.Twitter.E621Importer;

public interface ISourceRetriever
{
    IAsyncEnumerable<string> GetSourcesAsync();
}
