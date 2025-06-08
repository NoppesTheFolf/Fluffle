namespace Fluffle.Imaging.Api.Extensions;

public static class StreamExtensions
{
    public static async Task<MemoryStream> ToMemoryStreamAsync(this Stream stream)
    {
        var memoryStream = new MemoryStream();
        try
        {
            await stream.CopyToAsync(memoryStream);

            memoryStream.Position = 0;

            return memoryStream;
        }
        catch
        {
            await memoryStream.DisposeAsync();
            throw;
        }
    }
}
