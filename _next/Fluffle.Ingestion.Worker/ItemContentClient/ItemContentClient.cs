using Fluffle.Ingestion.Api.Models.Items;

namespace Fluffle.Ingestion.Worker.ItemContentClient;

public class ItemContentClient : IItemContentClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ItemContentClient> _logger;

    public ItemContentClient(IHttpClientFactory httpClientFactory, ILogger<ItemContentClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<Stream> DownloadAsync(ICollection<ImageModel> images)
    {
        var rankedImages = RankImages(images);

        using var httpClient = CreateClient();
        foreach (var image in rankedImages)
        {
            Stream? stream = null;
            try
            {
                _logger.LogInformation("Downloading image from URL {Url}...", image.Url);
                stream = await httpClient.GetStreamAsync(image.Url);
                return stream;
            }
            catch (Exception exception)
            {
                if (stream != null)
                    await stream.DisposeAsync();

                if (exception is not HttpRequestException httpRequestException)
                    throw;

                if (httpRequestException.StatusCode == null)
                    throw;

                _logger.LogWarning("Failed to download image from URL {Url} with status code {StatusCode}.", image.Url, httpRequestException.StatusCode);
            }
        }

        throw new Exception("No images could be downloaded.");
    }

    private static List<ImageModel> RankImages(ICollection<ImageModel> images)
    {
        // Prefer the smallest images with a decent resolution
        var rankedImages = images
            .Where(x => x is { Width: >= 400, Height: >= 400 })
            .OrderBy(x => x.Width * x.Height)
            .ToList();

        if (rankedImages.Count > 0)
            return rankedImages;

        // Else order by the largest and work from there
        rankedImages = images
            .OrderByDescending(x => x.Width * x.Height)
            .ToList();

        return rankedImages;
    }

    private HttpClient CreateClient()
    {
        var httpClient = _httpClientFactory.CreateClient(nameof(ItemContentClient));
        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("fluffle-ingestion-worker");

        return httpClient;
    }
}
