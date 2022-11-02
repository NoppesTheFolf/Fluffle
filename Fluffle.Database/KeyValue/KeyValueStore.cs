using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nito.AsyncEx;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Database.KeyValue
{
    public class KeyValueStore<TContext, TEntity> : IKeyValueStore where TContext : DbContext where TEntity : KeyValuePair, new()
    {
        private readonly IServiceProvider _services;
        private readonly AsyncLock _lock;

        public KeyValueStore(IServiceProvider services)
        {
            _services = services;
            _lock = new AsyncLock();
        }

        public async Task<KeyValueResult<T>> GetAsync<T>(string key)
        {
            return await UseKeyValueStoreAsync(async (_, set) =>
            {
                var entity = await set.SingleOrDefaultAsync(x => x.Key == key);
                if (entity == null)
                    return null;

                var value = JsonSerializer.Deserialize<T>(entity.Value);
                return new KeyValueResult<T>(value);
            });
        }

        public async Task SetAsync<T>(string key, T value)
        {
            await UseKeyValueStoreAsync(async (context, set) =>
            {
                var valueBytes = JsonSerializer.SerializeToUtf8Bytes(value);

                var entity = await set.SingleOrDefaultAsync(x => x.Key == key);
                if (entity == null)
                {
                    entity = new TEntity
                    {
                        Key = key
                    };
                    await set.AddAsync(entity);
                }

                entity.Value = valueBytes;
                await context.SaveChangesAsync();

                return string.Empty;
            });
        }

        private async Task<T> UseKeyValueStoreAsync<T>(Func<TContext, DbSet<TEntity>, Task<T>> operationAsync)
        {
            using var _ = await _lock.LockAsync();

            using var scope = _services.CreateScope();
            await using var context = scope.ServiceProvider.GetRequiredService<TContext>();
            var set = context.Set<TEntity>();
            var result = await operationAsync(context, set);

            return result;
        }
    }
}
