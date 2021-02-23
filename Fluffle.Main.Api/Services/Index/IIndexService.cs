using Noppes.Fluffle.Api.Services;
using Noppes.Fluffle.Main.Communication;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Main.Api.Services
{
    public interface IIndexService
    {
        public Task<SE> Index(string platformName, string idOnPlatform, PutImageIndexModel model);
    }
}
