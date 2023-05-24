using System.Threading.Tasks;

namespace Noppes.Fluffle.Api.RunnableServices;

/// <summary>
/// Allows services to be shutdown gracefully.
/// </summary>
public interface IShutdownable
{
    public Task ShutdownAsync();
}
