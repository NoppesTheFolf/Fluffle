using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Nito.AsyncEx;
using Noppes.Fluffle.Api.AccessControl;
using Noppes.Fluffle.Api.Controllers;
using Noppes.Fluffle.Api.Mapping;
using Noppes.Fluffle.FurAffinity;
using Noppes.Fluffle.Main.Communication;
using Noppes.Fluffle.Main.Database.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Main.Api.Controllers
{
    public class FurAffinityController : ApiControllerV1
    {
        public const string SingularRoute = Endpoints.FurAffinity;

        private const string RegisteredUsersKey = "_BotsAllowed";
        private static readonly TimeSpan ExpirationInterval = 10.Minutes();
        private static readonly AsyncLock Mutex = new();

        private readonly FluffleContext _context;
        private readonly FurAffinityClient _client;
        private readonly IMemoryCache _cache;

        public FurAffinityController(FluffleContext context, FurAffinityClient client, IMemoryCache cache)
        {
            _context = context;
            _client = client;
            _cache = cache;
        }

        [HttpGet(SingularRoute + "/bots-allowed")]
        [Permissions(FurAffinityPermissions.ViewBotsAllowed)]
        public async Task<IActionResult> GetStatus()
        {
            using var _ = await Mutex.LockAsync();

            if (!_cache.TryGetValue<int>(RegisteredUsersKey, out var registeredUsers))
            {
                registeredUsers = (await _client.GetRegisteredUsersOnlineAsync()).Registered;
                _cache.Set(RegisteredUsersKey, registeredUsers, ExpirationInterval);
            }

            return Ok(registeredUsers < FurAffinityClient.BotThreshold);
        }

        [HttpGet(SingularRoute + "/popular-artists")]
        public IActionResult GetPopularArtists()
        {
            var result = _context.FaPopularArtists.MapEnumerableTo<FaPopularArtistModel>();

            return Ok(result.ToList());
        }
    }

    public class FurAffinityPermissions : Permissions
    {
        public const string Prefix = "FUR_AFFINITY_";

        [Permission]
        public const string ViewBotsAllowed = Prefix + "VIEW_BOTS_ALLOWED";
    }
}
