using System.Threading.Tasks;

namespace Noppes.Fluffle.Api.RunnableServices
{
    /// <summary>
    /// Defines a service which can do work.
    /// </summary>
    public interface IService
    {
        public Task RunAsync();
    }
}
