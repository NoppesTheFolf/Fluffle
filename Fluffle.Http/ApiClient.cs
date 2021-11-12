using Flurl.Http;
using System;
using System.Collections.Generic;

namespace Noppes.Fluffle.Http
{
    /// <summary>
    /// Simple base class for any API clients.
    /// </summary>
    public abstract class ApiClient : IDisposable
    {
        private readonly List<ICallInterceptor> _interceptors;

        /// <summary>
        /// The HTTP client used to make requests with.
        /// </summary>
        protected IFlurlClient FlurlClient { get; }

        protected ApiClient(string baseUrl)
        {
            FlurlClient = new FlurlClient(baseUrl);

            _interceptors = new List<ICallInterceptor>();
        }

        public void AddInterceptor<T>() where T : ICallInterceptor, new() => AddInterceptor(new T());

        public void AddInterceptor(ICallInterceptor interceptor) => _interceptors.Add(interceptor);

        /// <summary>
        /// Create a new request by combing the base url and provided url segments.
        /// </summary>
        public virtual IFlurlRequest Request(params object[] urlSegments)
        {
            var request = FlurlClient.Request(urlSegments);

            foreach (var interceptor in _interceptors)
                request.AddInterceptor(interceptor);

            return request;
        }

        public void Dispose()
        {
            FlurlClient.Dispose();

            GC.SuppressFinalize(this);
        }
    }
}
