﻿using Flurl.Http;
using MessagePack;
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
        private static readonly MessagePackSerializerOptions Options =
            MessagePackSerializerOptions.Standard.WithSecurity(MessagePackSecurity.UntrustedData);

        /// <summary>
        /// Make a GET request. Deserialize the response using MessagePack format.
        /// </summary>
        public static async Task<T> GetMessagePackAsync<T>(this IFlurlRequest request, CancellationToken cancellationToken = default)
        {
            request.WithHeader("Accept", "application/x-msgpack");

            var responseStream = await request.GetStreamAsync(cancellationToken);

            return await MessagePackSerializer.DeserializeAsync<T>(responseStream, Options, cancellationToken);
        }

        /// <summary>
        /// Make a POST request with the data serialized as JSON. Deserialize the response using JSON.
        /// </summary>
        public static async Task<T> PostJsonReceiveJsonAsync<T>(this IFlurlRequest request, object data, CancellationToken cancellationToken = default)
        {
            var response = await request.PostJsonAsync(data, cancellationToken);

            return await response.GetJsonAsync<T>();
        }

        /// <summary>
        /// Make a POST request with the data provided by the <see cref="HttpContent"/> object.
        /// Deserialize the response using JSON.
        /// </summary>
        public static async Task<T> PostContentReceiveJsonAsync<T>(this IFlurlRequest request, HttpContent content = null, CancellationToken cancellationToken = default)
        {
            var response = await request.PostAsync(content, cancellationToken);

            return await response.GetJsonAsync<T>();
        }
    }
}