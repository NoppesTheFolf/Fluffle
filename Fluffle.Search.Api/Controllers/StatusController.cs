using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Nito.AsyncEx;
using Noppes.Fluffle.Main.Communication;
using Noppes.Fluffle.Search.Api.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Search.Api.Controllers
{
    public class StatusController : SearchApiControllerV1
    {
        private const string StatusCacheKey = "_Status";
        private static readonly TimeSpan ExpirationInterval = 4.Seconds();
        private static readonly AsyncLock Mutex = new();

        private readonly IStatusService _statusService;
        private readonly IMemoryCache _cache;

        public StatusController(IStatusService statusService, IMemoryCache cache)
        {
            _statusService = statusService;
            _cache = cache;
        }

        [AllowAnonymous]
        [HttpGet("status")]
        public async Task<IActionResult> GetStatus()
        {
            using var _ = await Mutex.LockAsync();

            if (_cache.TryGetValue<IList<StatusModel>>(StatusCacheKey, out var model))
                return Ok(model);

            model = await _statusService.GetStatusAsync();
            _cache.Set(StatusCacheKey, model, ExpirationInterval);

            return Ok(model);
        }
    }
}
