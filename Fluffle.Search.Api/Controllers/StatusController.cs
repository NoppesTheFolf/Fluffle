using Microsoft.AspNetCore.Mvc;
using Noppes.Fluffle.Main.Client;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Search.Api.Controllers
{
    public class StatusController : SearchApiControllerV1
    {
        private readonly FluffleClient _client;

        public StatusController(FluffleClient client)
        {
            _client = client;
        }

        [HttpGet("status")]
        public async Task<IActionResult> Index()
        {
            var status = await _client.GetStatusAsync();

            return Ok(status);
        }
    }
}
