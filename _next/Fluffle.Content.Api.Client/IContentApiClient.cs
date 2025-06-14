namespace Fluffle.Content.Api.Client;

public interface IContentApiClient
{
    Task PutAsync(string path, Stream stream);

    Task DeleteAsync(string path);
}
