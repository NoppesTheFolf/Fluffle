namespace Noppes.Fluffle.Client;

/// <summary>
/// API wrapper which talks with the API at https://api.fluffle.xyz.
/// </summary>
public interface IFluffleApiClient
{
    /// <summary>
    /// Reverse search an image, provided as a stream, using Fluffle.
    /// </summary>
    /// <param name="file">
    /// The image to reverse search. The currently supported image formats are JPEG, PNG, GIF and
    /// WebP. The size of the provided image must not exceed 4 MiB, nor should the image its
    /// dimensions exceed a total area (width * height) of 16 million pixels (16MP).
    /// </param>
    /// <param name="includeNsfw">
    /// An optional boolean value indicating whether or not Fluffle should also search through
    /// images deemed Not Safe For Work. By default, Fluffle will not search NSFW images. Fluffle is
    /// unable to reliably determine if images from Twitter are NSFW or not. Therefore, to play it
    /// safe, all images are considered explicit by default. You should include NSFW results if you
    /// want to use Twitter as a source.
    /// </param>
    /// <param name="platforms">
    /// The platforms to be included in the reverse search process. Not providing this field will
    /// cause all platforms to be included. Make sure your application can deal with new
    /// platforms: the API its version will not be increased if it is decided to add support for
    /// another platform. An alternative would be to send a list of all platforms your application
    /// supports with each request. The values provided to this field are casing-insensitive,
    /// meaning you can pass them snake cased, camel cased, or whatever you prefer.
    /// </param>
    /// <param name="limit">
    /// By default, Fluffle will send you a maximum of 32 results. You can tweak this number by
    /// providing a value from 8 to 32. Exceeding this range will cause your request to fail.
    /// </param>
    /// <param name="createLink">
    /// Whether or not Fluffle should store your search result indefinitely and make it accessible
    /// through a link. This parameter defaults to false. After all, it takes up resources to make
    /// the search result available. Because of this, please do not set this value to true if a user
    /// did not (implicitly or explicitly) request the search result to be used this way. The
    /// response will contain an ID. Appending this ID to the end of https://fluffle.xyz/q/ (e.g.
    /// https://fluffle.xyz/q/abc) will allow the search result to be viewed in a browser.
    /// </param>
    Task<FluffleSearchResponse> ReverseSearchAsync(Stream file, bool? includeNsfw = null, IEnumerable<FlufflePlatform>? platforms = null, int? limit = null, bool? createLink = null);
}
