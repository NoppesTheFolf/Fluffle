using System;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Utils;

/// <summary>
/// Base class for all producers. The <see cref="OnProducedAsync"/> method can be overwritten to
/// implement custom logic when the instance produces an object of type <typeparamref name="T"/>
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class Producer<T>
{
    public ChannelWriter<T> Output { get; set; }

    public async Task RunAsync()
    {
        while (true)
        {
            await WorkAsync();
        }
    }

    public abstract Task WorkAsync();

    public async Task ProduceAsync(T produced)
    {
        var mustContinue = await OnProducedAsync(produced);

        if (!mustContinue || Output == null)
        {
            if (produced is IDisposable disposable)
                disposable.Dispose();

            return;
        }

        await Output.WriteAsync(produced);
    }

    public virtual Task<bool> OnProducedAsync(T produced)
    {
        return Task.FromResult(true);
    }
}
