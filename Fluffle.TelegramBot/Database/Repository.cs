using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Fluffle.TelegramBot.Database;

public interface IRepository<T> where T : class
{
    public Task InsertAsync(T document);

    public Task ReplaceAsync(Expression<Func<T, bool>> predicate, T document);

    public Task UpsertAsync(Expression<Func<T, bool>> predicate, T document);

    public Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate) => QueryFirstOrDefaultAsync(x => x.Where(predicate));

    public Task<T> QueryFirstOrDefaultAsync(Func<IMongoQueryable<T>, IMongoQueryable<T>> applyFilter);

    public Task<T> FirstAsync(Expression<Func<T, bool>> predicate) => QueryFirstAsync(x => x.Where(predicate));

    public Task<T> QueryFirstAsync(Func<IMongoQueryable<T>, IMongoQueryable<T>> applyFilter);

    public Task<List<T>> ManyAsync(Expression<Func<T, bool>> predicate) => QueryManyAsync(x => x.Where(predicate));

    public Task<List<T>> QueryManyAsync(Func<IMongoQueryable<T>, IMongoQueryable<T>> applyFilter);

    public Task<long> DeleteManyAsync(Expression<Func<T, bool>> predicate);

    public Task<bool> AnyAsync(Expression<Func<T, bool>> predicate) => QueryAnyAsync(x => x.Where(predicate));

    public Task<bool> QueryAnyAsync(Func<IMongoQueryable<T>, IMongoQueryable<T>> applyFilter);
}

public abstract class Repository<T> : IRepository<T> where T : class, new()
{
    private readonly IMongoCollection<T> _collection;

    protected Repository(IMongoCollection<T> collection)
    {
        _collection = collection;
    }

    public Task InsertAsync(T document)
    {
        return _collection.InsertOneAsync(document);
    }

    public Task ReplaceAsync(Expression<Func<T, bool>> predicate, T document)
    {
        return _collection.ReplaceOneAsync(predicate, document);
    }

    public Task<List<T>> QueryManyAsync(Func<IMongoQueryable<T>, IMongoQueryable<T>> applyFilter)
    {
        return applyFilter(_collection.AsQueryable()).ToListAsync();
    }

    public async Task<long> DeleteManyAsync(Expression<Func<T, bool>> predicate)
    {
        var result = await _collection.DeleteManyAsync(predicate);

        return result.DeletedCount;
    }

    public Task<bool> QueryAnyAsync(Func<IMongoQueryable<T>, IMongoQueryable<T>> applyFilter)
    {
        return applyFilter(_collection.AsQueryable()).AnyAsync();
    }

    public Task<T> QueryFirstAsync(Func<IMongoQueryable<T>, IMongoQueryable<T>> applyFilter)
    {
        return applyFilter(_collection.AsQueryable()).FirstAsync();
    }

    public Task<T> QueryFirstOrDefaultAsync(Func<IMongoQueryable<T>, IMongoQueryable<T>> applyFilter)
    {
        return applyFilter(_collection.AsQueryable()).FirstOrDefaultAsync();
    }

    public Task UpsertAsync(Expression<Func<T, bool>> predicate, T document)
    {
        return _collection.ReplaceOneAsync(predicate, document, new ReplaceOptions { IsUpsert = true });
    }
}
