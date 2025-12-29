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

    [HttpPut("/{*path}")]
    public async Task<IActionResult> PutContentAsync(string? path)
    {
        path = path?.TrimStart('/');

        if (string.IsNullOrWhiteSpace(path))
            return BadRequest("Invalid path was provided.");

        using var memoryStream = new MemoryStream();
        await HttpContext.Request.Body.CopyToAsync(memoryStream);

        if (memoryStream.Length == 0)
            return BadRequest("No body was provided.");

        memoryStream.Position = 0;
        await _storage.PutAsync(path, memoryStream);

        return Ok();
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

    [HttpDelete("/{*path}")]
    public async Task<IActionResult> DeleteContentAsync(string? path)
    {
        path = path?.TrimStart('/');

        if (string.IsNullOrWhiteSpace(path))
            return BadRequest("Invalid path was provided.");

        await _storage.DeleteAsync(path);

        return Ok();
    }
}
