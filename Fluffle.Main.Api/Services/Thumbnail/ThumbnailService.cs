using Flurl.Http;
using Microsoft.Extensions.Logging;
using Noppes.Fluffle.Api.Services;
using Noppes.Fluffle.B2;
using Noppes.Fluffle.Http;
using Noppes.Fluffle.Main.Database.Models;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Main.Api.Services;

public class ThumbnailService : Service, IThumbnailService
{
    private readonly FluffleContext _context;
    private readonly B2Bucket _bucket;
    private readonly ILogger<ThumbnailService> _logger;

    public ThumbnailService(FluffleContext context, B2Bucket bucket, ILogger<ThumbnailService> logger)
    {
        _context = context;
        _bucket = bucket;
        _logger = logger;
    }

    public async Task DeleteAsync(Thumbnail thumbnail, bool save = true)
    {
        try
        {
            await HttpResiliency.RunAsync(() =>
                _bucket.DeleteFileVersionAsync(thumbnail.Filename, thumbnail.B2FileId));

            _context.Thumbnails.Remove(thumbnail);

            if (save)
                await _context.SaveChangesAsync();
        }
        catch (FlurlHttpException httpException)
        {
            if (httpException.Call.Response == null)
                throw;

            var error = await httpException.Call.Response.GetJsonAsync<B2ErrorResponse>();

            // The file has already been deleted, no problemo
            if (error.Code != B2ErrorCode.FileNotPresent)
                throw;

            _logger.LogWarning(
                "Thumbnail with filename {filename} and B2 file ID {b2fileId} was not found and therefore could not be deleted.",
                thumbnail.Filename, thumbnail.B2FileId);
        }
    }
}
