using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Database.Queue;

public class MessageQueue<TContext, TEntity, TData> where TContext : DbContext where TEntity : QueueEntity, new() where TData : new()
{
    private readonly IServiceProvider _services;

    public MessageQueue(IServiceProvider services)
    {
        _services = services;
    }

    public async Task<QueueItem<TData>> EnqueueAsync(TData data, long priority)
    {
        var entity = await UseQueueAsync(async (context, set) =>
        {
            var entity = new TEntity
            {
                Data = JsonSerializer.SerializeToUtf8Bytes(data),
                Priority = priority
            };

            await set.AddAsync(entity);
            await context.SaveChangesAsync();

            return entity;
        });

        return new QueueItem<TData>
        {
            Id = entity.Id,
            Data = data
        };
    }

    public async Task<QueueItem<TData>> DequeueAsync()
    {
        var entity = await UseQueueAsync(async (_, set) =>
        {
            var entity = await set.OrderBy(x => x.Priority).FirstOrDefaultAsync();

            return entity;
        });

        if (entity == null)
            return null;

        var data = JsonSerializer.Deserialize<TData>(entity.Data);
        return new QueueItem<TData>
        {
            Id = entity.Id,
            Data = data
        };
    }

    public async Task<bool> AcknowledgeAsync(QueueItem<TData> item)
    {
        var removed = await UseQueueAsync(async (context, set) =>
        {
            var entity = await set.SingleOrDefaultAsync(x => x.Id == item.Id);
            if (entity == null)
                return false;

            set.Remove(entity);
            await context.SaveChangesAsync();

            return true;
        });

        return removed;
    }

    private async Task<TResult> UseQueueAsync<TResult>(Func<TContext, DbSet<TEntity>, Task<TResult>> operationAsync)
    {
        using var scope = _services.CreateScope();
        await using var context = scope.ServiceProvider.GetRequiredService<TContext>();
        var set = context.Set<TEntity>();
        var result = await operationAsync(context, set);

        return result;
    }
}
