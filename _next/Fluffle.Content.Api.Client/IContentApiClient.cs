namespace Fluffle.Content.Api.Client;

public interface IContentApiClient
{
    Task PutAsync(string path, Stream stream);

    Task<Stream?> GetAsync(string path);

    Task DeleteAsync(string path);
}
