using Flurl.Http;
using System;

namespace Noppes.Fluffle.Http
{
    /// <summary>
    /// Simple base class for any API clients.
    /// </summary>
    public abstract class ApiClient : IDisposable
    {
        /// <summary>
        /// The HTTP client used to make requests with.
        /// </summary>
        private IFlurlClient FlurlClient { get; }

        protected ApiClient(string baseUrl)
        {
            FlurlClient = new FlurlClient(baseUrl);
        }

        /// <summary>
        /// Create a new request by combing the base url and provided url segments.
        /// </summary>
        public virtual IFlurlRequest Request(params object[] urlSegments)
        {
            return FlurlClient.Request(urlSegments);
        }

        public void Dispose()
        {
            FlurlClient.Dispose();

            GC.SuppressFinalize(this);
        }
    }
}
