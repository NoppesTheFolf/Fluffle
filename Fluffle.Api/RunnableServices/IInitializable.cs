using System.Threading.Tasks;

namespace Noppes.Fluffle.Api.RunnableServices
{
    /// <summary>
    /// Adds an initialization step to an <see cref="IService"/>. This can be especially handy if a
    /// service is also a singleton.
    /// </summary>
    public interface IInitializable
    {
        public Task InitializeAsync();
    }
}
