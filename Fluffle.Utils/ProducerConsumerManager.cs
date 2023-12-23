using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Utils;

/// <summary>
/// Helper class to make dealing with producers and consumers just a teeny tiny bit less of a
/// pain in the ass. Consumers and producers need to be transiently registered to the provided
/// <see cref="IServiceProvider"/> object. The dependencies required by the producers and
/// consumers will therefore also be resolved through dependency injection. The moment a new
/// producer/consumer gets added, they're started immediately.
/// </summary>
/// <typeparam name="T">The communication type used between producers and consumers.</typeparam>
public class ProducerConsumerManager<T>
{
    private readonly IServiceProvider _services;
    private readonly ICollection<Task> _tasks;

    private readonly ChannelWriter<T> _start;
    private Channel<T> _currentChannel;

    public ProducerConsumerManager(IServiceProvider services, int producerCapacity)
    {
        _tasks = new List<Task>();
        _services = services;
        _currentChannel = Channel.CreateBounded<T>(producerCapacity);
        _start = _currentChannel.Writer;
    }

    /// <summary>
    /// Adds a producer to the start of the chain.
    /// </summary>
    public void AddProducer<TProducer>(int count) where TProducer : Producer<T> =>
        AddProducer(count, () => _services.GetRequiredService<TProducer>());

    /// <summary>
    /// Adds a producer to the start of the chain.
    /// </summary>
    public void AddProducer<TProducer>(int count, Func<TProducer> createProducer) where TProducer : Producer<T>
    {
        for (var i = 0; i < count; i++)
        {
            var producer = createProducer();

            producer.Output = _start;

            _tasks.Add(Task.Run(producer.RunAsync));
        }
    }

    /// <summary>
    /// Adds a consumer to the chain.
    /// </summary>
    public void AddConsumer<TConsumer>(int count, int capacity) where TConsumer : Consumer<T>
    {
        var outputChannel = Channel.CreateBounded<T>(capacity);
        for (var i = 0; i < count; i++)
        {
            var consumer = _services.GetRequiredService<TConsumer>();

            consumer.Input = _currentChannel.Reader;
            consumer.Output = outputChannel.Writer;

            _tasks.Add(Task.Run(consumer.RunAsync));
        }

        _currentChannel = outputChannel;
    }

    /// <summary>
    /// Adds a consumer at the end of the chain.
    /// </summary>
    public void AddFinalConsumer<TConsumer>(int count) where TConsumer : Consumer<T>
    {
        for (var i = 0; i < count; i++)
        {
            var consumer = _services.GetRequiredService<TConsumer>();

            consumer.Input = _currentChannel.Reader;

            _tasks.Add(Task.Run(consumer.RunAsync));
        }
    }

    /// <summary>
    /// Returns an indefinitely running task waiting for any of the producers and/or consumers
    /// to complete. By design, producers/consumers should never complete, so if they do, that
    /// means something went bad, likely due to the consumer/producer in question throwing an exception.
    /// </summary>
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var task = await Task.WhenAny(_tasks).WaitAsync(cancellationToken); // Should really rethink the architecture of how the consumer/producer pattern is implemented here

        if (task.Exception == null)
            throw new InvalidOperationException();

        throw task.Exception;
    }
}
