using System.Threading.Channels;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Utils;

/// <summary>
/// Base class for all consumers. Also functions as a <see cref="Producer{T}"/> if the consumer
/// has an output to write to.
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class Consumer<T> : Producer<T>
{
    public ChannelReader<T> Input { get; set; }

    public override async Task WorkAsync()
    {
        var input = await Input.ReadAsync();

        var output = await ConsumeAsync(input);
        await ProduceAsync(output);
    }

    public abstract Task<T> ConsumeAsync(T toConsume);
}
