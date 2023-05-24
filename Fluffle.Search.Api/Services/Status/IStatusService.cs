using Noppes.Fluffle.Main.Communication;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Search.Api.Services;

public interface IStatusService
{
    Task<IList<StatusModel>> GetStatusAsync();
}
