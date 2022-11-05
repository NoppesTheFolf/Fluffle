using Azure.Storage.Queues;
using Nito.AsyncEx;

namespace Noppes.Fluffle.Queue.Azure;

internal interface IQueueClientProvider
{
    QueueClient Get(string name);
}

internal class QueueClientProvider : IQueueClientProvider
{
    private readonly string _connectionString;
    private readonly IDictionary<string, QueueClient> _clients;
    private readonly AsyncLock _lock;

    public QueueClientProvider(string connectionString)
    {
        _connectionString = connectionString;
        _clients = new Dictionary<string, QueueClient>();
        _lock = new AsyncLock();
    }

    public QueueClient Get(string name)
    {
        using var _ = _lock.Lock();
        if (_clients.TryGetValue(name, out var client))
            return client;

        client = new QueueClient(_connectionString, name);
        client.CreateIfNotExists();

        _clients[name] = client;

        return client;
    }
}