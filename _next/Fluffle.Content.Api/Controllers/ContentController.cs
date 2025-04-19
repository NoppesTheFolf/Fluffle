using Fluffle.Content.Api.Storage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace Fluffle.Content.Api.Controllers;

public class ContentController : ControllerBase
{
    private static readonly FileExtensionContentTypeProvider ContentTypeProvider = new();

    private readonly FtpStorage _storage;

    public ContentController(FtpStorage storage)
    {
        _storage = storage;
    }

    [HttpGet("/{*path}")]
    public async Task<IActionResult> GetContentAsync(string? path)
    {
        path = path?.TrimStart('/');

        if (string.IsNullOrWhiteSpace(path))
            return NotFound();

        var stream = await _storage.GetAsync(path);
        if (stream == null)
            return NotFound();

        if (!ContentTypeProvider.TryGetContentType(path, out var contentType))
            contentType = "application/octet-stream";

        return new FileStreamResult(stream, contentType);
    }
}
