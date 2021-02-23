using Nito.AsyncEx;
using Noppes.Fluffle.Main.Database.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Main.Api.Helpers
{
    public class ChangeIdIncrementer<T> where T : class, ITrackable
    {
        private bool _initialized;
        private long _changeId;
        private readonly AsyncLock _lock = new AsyncLock();

        public async Task NextAsync(T target)
        {
            target.ChangeId = await NextAsync();
        }

        public async Task<long> NextAsync()
        {
            using var _ = await _lock.LockAsync();

            if (!_initialized)
                throw new InvalidOperationException();

            return ++_changeId;
        }

        public void Initialize(FluffleContext context)
        {
            using var _ = _lock.Lock();

            if (_initialized)
                throw new InvalidOperationException();

            var set = context.Set<T>();
            var maxChangeId = set.Max(i => i.ChangeId) ?? 0;

            _changeId = maxChangeId;
            _initialized = true;
        }
    }
}
