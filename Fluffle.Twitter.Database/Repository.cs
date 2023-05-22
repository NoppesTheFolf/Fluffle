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

    public Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate) => QueryFirstOrDefaultAsync(x => x.Where(predicate));

    public Task<T?> QueryFirstOrDefaultAsync(Func<IMongoQueryable<T>, IMongoQueryable<T>> applyFilter);

    public Task<T> FirstAsync(Expression<Func<T, bool>> predicate) => QueryFirstAsync(x => x.Where(predicate));

    public Task<T> QueryFirstAsync(Func<IMongoQueryable<T>, IMongoQueryable<T>> applyFilter);

    public Task<List<T>> ManyAsync(Expression<Func<T, bool>> predicate) => QueryManyAsync(x => x.Where(predicate));

    public Task<List<T>> QueryManyAsync(Func<IMongoQueryable<T>, IMongoQueryable<T>> applyFilter);

    public Task<long> DeleteManyAsync(Expression<Func<T, bool>> predicate);

    public Task<bool> AnyAsync(Expression<Func<T, bool>> predicate) => QueryAnyAsync(x => x.Where(predicate));

    public Task<bool> QueryAnyAsync(Func<IMongoQueryable<T>, IMongoQueryable<T>> applyFilter);
}

public class MongoRepository<T> : IMongoRepository<T> where T : class, new()
{
    public IMongoCollection<T> Collection { get; }

    public MongoRepository(IMongoCollection<T> collection)
    {
        Collection = collection;
    }

    public Task InsertAsync(T document)
    {
        return Collection.InsertOneAsync(document);
    }

    public Task ReplaceAsync(Expression<Func<T, bool>> predicate, T document)
    {
        return Collection.ReplaceOneAsync(predicate, document);
    }

    public Task<List<T>> QueryManyAsync(Func<IMongoQueryable<T>, IMongoQueryable<T>> applyFilter)
    {
        return applyFilter(Collection.AsQueryable()).ToListAsync();
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

    public Task<T?> QueryFirstOrDefaultAsync(Func<IMongoQueryable<T>, IMongoQueryable<T>> applyFilter)
    {
        return applyFilter(Collection.AsQueryable()).FirstOrDefaultAsync()!;
    }

    public Task UpsertAsync(Expression<Func<T, bool>> predicate, T document)
    {
        return Collection.ReplaceOneAsync(predicate, document, new ReplaceOptions { IsUpsert = true });
    }
}