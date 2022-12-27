using Flurl.Http;
using Flurl.Http.Content;
using HtmlAgilityPack;
using MessagePack;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Http
{
    /// <summary>
    /// Has extension methods for objects implementing the <see cref="IFlurlRequest"/> interface.
    /// These include
    /// </summary>
    public static class FlurlExtensions
    {
        /// <summary>
        /// The settings used when deserializing data in the MessagePack format.
        /// </summary>
        private static readonly MessagePackSerializerOptions Options = MessagePackSerializerOptions.Standard.WithSecurity(MessagePackSecurity.UntrustedData);

        public static async Task<T> GetJsonExplicitlyAsync<T>(this IFlurlRequest request, CancellationToken cancellationToken = default)
        {
            return await request.AcceptJson().GetJsonAsync<T>();
        }

        public static async Task<T> PostMultipartReceiveJsonExplicitlyAsync<T>(this IFlurlRequest request, Action<CapturedMultipartContent> buildContent, CancellationToken cancellationToken = default)
        {
            var response = await request.AcceptJson().PostMultipartAsync(buildContent, cancellationToken);

            return await response.GetJsonAsync<T>();
        }

        /// <summary>
        /// Make a GET request. Deserialize the response using MessagePack format.
        /// </summary>
        public static async Task<T> GetMessagePackExplicitlyAsync<T>(this IFlurlRequest request, CancellationToken cancellationToken = default)
        {
            var responseStream = await request.AcceptMessagePack().GetStreamAsync(cancellationToken);

            return await MessagePackSerializer.DeserializeAsync<T>(responseStream, Options, cancellationToken);
        }

        /// <summary>
        /// Make a DELETE request with the data serialized as JSON. Deserialize the response using JSON.
        /// </summary>
        public static async Task<T> DeleteJsonReceiveJsonExplicitlyAsync<T>(this IFlurlRequest request, object data, CancellationToken cancellationToken = default)
        {
            var response = await request
                .AcceptJson()
                .DeleteJsonAsync(data, cancellationToken);

            return await response.GetJsonAsync<T>();
        }

        /// <summary>
        /// Make a DELETE request with the data serialized as JSON.
        /// </summary>
        public static Task<IFlurlResponse> DeleteJsonAsync(this IFlurlRequest request, object data, CancellationToken cancellationToken = default)
        {
            var content = new CapturedJsonContent(request.Settings.JsonSerializer.Serialize(data));

            return request.SendAsync(HttpMethod.Delete, content, cancellationToken);
        }

        /// <summary>
        /// Make a POST request with the data serialized as JSON. Deserialize the response using JSON.
        /// </summary>
        public static async Task<T> PostJsonReceiveJsonExplicitlyAsync<T>(this IFlurlRequest request, object data, CancellationToken cancellationToken = default)
        {
            var response = await request.AcceptJson().PostJsonAsync(data, cancellationToken);

            return await response.GetJsonAsync<T>();
        }

        /// <summary>
        /// Make a POST request with the data provided by the <see cref="HttpContent"/> object.
        /// Deserialize the response using JSON.
        /// </summary>
        public static async Task<T> PostContentReceiveJsonExplicitlyAsync<T>(this IFlurlRequest request, HttpContent content = null, CancellationToken cancellationToken = default)
        {
            var response = await request.AcceptJson().PostAsync(content, cancellationToken);

            return await response.GetJsonAsync<T>();
        }

        /// <summary>
        /// Make a GET request and parse the response as a HTML document.
        /// </summary>
        public static async Task<HtmlDocument> GetHtmlExplicitlyAsync(this IFlurlRequest request)
        {
            var response = await request.AcceptHtml().GetStreamAsync();

            var document = new HtmlDocument();
            document.Load(response);

            return document;
        }

        public static IFlurlRequest AcceptJson(this IFlurlRequest request)
        {
            return request.WithHeader("Accept", "application/json");
        }

        public static IFlurlRequest AcceptMessagePack(this IFlurlRequest request)
        {
            return request.WithHeader("Accept", "application/x-msgpack");
        }

        public static IFlurlRequest AcceptHtml(this IFlurlRequest request)
        {
            return request.WithHeader("Accept", "text/html");
        }

        /// <summary>
        /// Adds an interceptor to the request.
        /// </summary>
        public static IFlurlRequest AddInterceptor(this IFlurlRequest request, ICallInterceptor interceptor)
        {
            return request
                .BeforeCall(interceptor.InterceptBeforeAsync)
                .AfterCall(interceptor.InterceptAfterAsync);
        }
    }
}
