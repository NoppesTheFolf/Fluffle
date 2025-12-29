using Microsoft.Extensions.Options;
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
    private readonly BotContext _context;

    public CallbackContextRepository(BotContext context)
    {
        _context = context;
    }

    public async Task<CallbackContext> GetAsync(string id) => await _context.CallbackContexts.AsQueryable().Where(x => x.Id == id).FirstOrDefaultAsync();

    public async Task DeleteAsync(string id) => await _context.CallbackContexts.DeleteOneAsync(x => x.Id == id);

    public async Task PutAsync(CallbackContext document) => await _context.CallbackContexts.ReplaceOneAsync(x => x.Id == document.Id, document, new ReplaceOptions { IsUpsert = true });
}

public class InputContextRepository : ITelegramRepository<InputContext, long>
{
    private readonly BotContext _context;

    public InputContextRepository(BotContext context)
    {
        _context = context;
    }

    public async Task<InputContext> GetAsync(long id) => await _context.InputContexts.AsQueryable().Where(x => x.Id == id).FirstOrDefaultAsync();

    public async Task DeleteAsync(long id) => await _context.InputContexts.DeleteOneAsync(x => x.Id == id);

    public async Task PutAsync(InputContext document) => await _context.InputContexts.ReplaceOneAsync(x => x.Id == document.Id, document, new ReplaceOptions { IsUpsert = true });
}

public class BotContext
{
    public BotContext(IOptions<BotConfiguration> options)
    {
        var mongoClient = new MongoClient(options.Value.MongoConnectionString);
        var mongoDatabase = mongoClient.GetDatabase(options.Value.MongoDatabase);

        Chats = new ChatRepository(mongoDatabase.GetCollection<MongoChat>("Chat"));
        Messages = new MessageRepository(mongoDatabase.GetCollection<MongoMessage>("Message"));

        ReverseSearchRequestHistory = new ReverseSearchHistoryRepository(mongoDatabase.GetCollection<MongoReverseSearchRequestHistory>("ReverseSearchRequestHistory"));

        CallbackContexts = mongoDatabase.GetCollection<CallbackContext>("CallbackContext");
        InputContexts = mongoDatabase.GetCollection<InputContext>("InputContext");
    }

    public IMongoCollection<CallbackContext> CallbackContexts { get; }
    public IMongoCollection<InputContext> InputContexts { get; }

    public IRepository<MongoChat> Chats { get; }
    public IRepository<MongoMessage> Messages { get; }

    public IRepository<MongoReverseSearchRequestHistory> ReverseSearchRequestHistory { get; }
}
