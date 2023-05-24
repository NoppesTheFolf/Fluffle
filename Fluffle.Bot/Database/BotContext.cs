using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Noppes.Fluffle.Bot.Routing;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Bot.Database;

public class ChatRepository : Repository<MongoChat>
{
    public ChatRepository(IMongoCollection<MongoChat> collection) : base(collection)
    {
    }
}

public class MessageRepository : Repository<MongoMessage>
{
    public MessageRepository(IMongoCollection<MongoMessage> collection) : base(collection)
    {
    }
}

public class ReverseSearchHistoryRepository : Repository<MongoReverseSearchRequestHistory>
{
    public ReverseSearchHistoryRepository(IMongoCollection<MongoReverseSearchRequestHistory> collection) : base(collection)
    {
    }
}

public class CallbackContextRepository : ITelegramRepository<CallbackContext, string>
{
    private readonly IMongoCollection<CallbackContext> _mongoCollection;

    public CallbackContextRepository(IMongoCollection<CallbackContext> mongoCollection)
    {
        _mongoCollection = mongoCollection;
    }

    public async Task<CallbackContext> GetAsync(string id) => await _mongoCollection.AsQueryable().Where(x => x.Id == id).FirstOrDefaultAsync();

    public async Task DeleteAsync(string id) => await _mongoCollection.DeleteOneAsync(x => x.Id == id);

    public async Task PutAsync(CallbackContext document) => await _mongoCollection.ReplaceOneAsync(x => x.Id == document.Id, document, new ReplaceOptions { IsUpsert = true });
}

public class InputContextRepository : ITelegramRepository<InputContext, long>
{
    private readonly IMongoCollection<InputContext> _mongoCollection;

    public InputContextRepository(IMongoCollection<InputContext> mongoCollection)
    {
        _mongoCollection = mongoCollection;
    }

    public async Task<InputContext> GetAsync(long id) => await _mongoCollection.AsQueryable().Where(x => x.Id == id).FirstOrDefaultAsync();

    public async Task DeleteAsync(long id) => await _mongoCollection.DeleteOneAsync(x => x.Id == id);

    public async Task PutAsync(InputContext document) => await _mongoCollection.ReplaceOneAsync(x => x.Id == document.Id, document, new ReplaceOptions { IsUpsert = true });
}

public class MediaGroupRepository : Repository<MongoMediaGroup>
{
    public MediaGroupRepository(IMongoCollection<MongoMediaGroup> collection) : base(collection)
    {
    }
}

public class BotContext
{
    private readonly IMongoClient _mongoClient;
    private readonly IMongoDatabase _mongoDatabase;

    public BotContext(string connectionString, string database)
    {
        _mongoClient = new MongoClient(connectionString);
        _mongoDatabase = _mongoClient.GetDatabase(database);

        Chats = new ChatRepository(_mongoDatabase.GetCollection<MongoChat>("Chat"));
        Messages = new MessageRepository(_mongoDatabase.GetCollection<MongoMessage>("Message"));
        MediaGroups = new MediaGroupRepository(_mongoDatabase.GetCollection<MongoMediaGroup>("MediaGroup"));

        ReverseSearchRequestHistory = new ReverseSearchHistoryRepository(_mongoDatabase.GetCollection<MongoReverseSearchRequestHistory>("ReverseSearchRequestHistory"));

        CallbackContexts = _mongoDatabase.GetCollection<CallbackContext>("CallbackContext");
        InputContexts = _mongoDatabase.GetCollection<InputContext>("InputContext");
    }

    public IMongoCollection<CallbackContext> CallbackContexts { get; }
    public IMongoCollection<InputContext> InputContexts { get; }

    public IRepository<MongoChat> Chats { get; }
    public IRepository<MongoMessage> Messages { get; }
    public IRepository<MongoMediaGroup> MediaGroups { get; }

    public IRepository<MongoReverseSearchRequestHistory> ReverseSearchRequestHistory { get; }
}
