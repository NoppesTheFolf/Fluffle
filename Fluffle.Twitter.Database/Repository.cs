using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Linq.Expressions;

namespace Noppes.Fluffle.Twitter.Database;

/// <summary>
/// Provides an opinionated interface to interact with MongoDB collections that is a little easier
/// to work with and more intuitive.
///
/// EDIT: Jk it is not easier to work with this interface was a mistake, but I'm too lazy to edit existing code :3
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IMongoRepository<T> where T : class
{
    public IMongoCollection<T> Collection { get; }

    public Task InsertAsync(T document);

    public Task ReplaceAsync(Expression<Func<T, bool>> predicate, T document);

    public Task UpsertAsync(Expression<Func<T, bool>> predicate, T document);

    public Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, bool caseInsensitive = false);

    public Task<T> FirstAsync(Expression<Func<T, bool>> predicate) => QueryFirstAsync(x => x.Where(predicate));

    public Task<T> QueryFirstAsync(Func<IMongoQueryable<T>, IMongoQueryable<T>> applyFilter);

    public Task<List<T>> ManyAsync(Expression<Func<T, bool>> predicate, bool caseInsensitive = false);

    public Task<long> DeleteManyAsync(Expression<Func<T, bool>> predicate);

    public Task<bool> AnyAsync(Expression<Func<T, bool>> predicate) => QueryAnyAsync(x => x.Where(predicate));

    public Task<bool> QueryAnyAsync(Func<IMongoQueryable<T>, IMongoQueryable<T>> applyFilter);
}

public class MongoRepository<T> : IMongoRepository<T> where T : class, new()
{
    public IMongoCollection<T> Collection { get; }

    private readonly Collation _caseInsensitiveCollation;

    public MongoRepository(IMongoCollection<T> collection, Collation caseInsensitiveCollation)
    {
        Collection = collection;
        _caseInsensitiveCollation = caseInsensitiveCollation;
    }

    public Task InsertAsync(T document)
    {
        return Collection.InsertOneAsync(document);
    }

    public Task ReplaceAsync(Expression<Func<T, bool>> predicate, T document)
    {
        return Collection.ReplaceOneAsync(predicate, document);
    }

    public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, bool caseInsensitive = false)
    {
        using var cursor = await GetCursorAsync(predicate, 1, caseInsensitive);
        var entity = await cursor.FirstOrDefaultAsync();

        return entity;
    }

    public async Task<List<T>> ManyAsync(Expression<Func<T, bool>> predicate, bool caseInsensitive = false)
    {
        using var cursor = await GetCursorAsync(predicate, null, caseInsensitive);
        var entities = await cursor.ToListAsync();

        return entities;
    }

    private async Task<IAsyncCursor<T>> GetCursorAsync(Expression<Func<T, bool>> predicate, int? limit = null, bool caseInsensitive = false)
    {
        var filter = Builders<T>.Filter.Where(predicate);

        var findOptions = new FindOptions<T>
        {
            Limit = limit
        };
        if (caseInsensitive)
            findOptions.Collation = _caseInsensitiveCollation;

        var cursor = await Collection.FindAsync(filter, findOptions);
        return cursor;
    }

    public async Task<long> DeleteManyAsync(Expression<Func<T, bool>> predicate)
    {
        var result = await Collection.DeleteManyAsync(predicate);

        return result.DeletedCount;
    }

    public Task<bool> QueryAnyAsync(Func<IMongoQueryable<T>, IMongoQueryable<T>> applyFilter)
    {
        return applyFilter(Collection.AsQueryable()).AnyAsync();
    }

    public Task<T> QueryFirstAsync(Func<IMongoQueryable<T>, IMongoQueryable<T>> applyFilter)
    {
        return applyFilter(Collection.AsQueryable()).FirstAsync();
    }

    public Task UpsertAsync(Expression<Func<T, bool>> predicate, T document)
    {
        return Collection.ReplaceOneAsync(predicate, document, new ReplaceOptions { IsUpsert = true });
    }
}
