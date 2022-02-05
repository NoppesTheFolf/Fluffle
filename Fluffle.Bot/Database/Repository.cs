using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Noppes.Fluffle.Bot.Database
{
    public interface IRepository<T> where T : class
    {
        public Task InsertAsync(T document);

        public Task ReplaceAsync(Expression<Func<T, bool>> predicate, T document);

        public Task UpsertAsync(Expression<Func<T, bool>> predicate, T document);

        public Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate) => FirstOrDefaultAsync(x => x.Where(predicate));

        public Task<T> FirstOrDefaultAsync(Func<IMongoQueryable<T>, IMongoQueryable<T>> applyFilter);

        public Task<T> FirstAsync(Expression<Func<T, bool>> predicate) => FirstAsync(x => x.Where(predicate));

        public Task<T> FirstAsync(Func<IMongoQueryable<T>, IMongoQueryable<T>> applyFilter);

        public Task<List<T>> ManyAsync(Expression<Func<T, bool>> predicate) => ManyAsync(x => x.Where(predicate));

        public Task<List<T>> ManyAsync(Func<IMongoQueryable<T>, IMongoQueryable<T>> applyFilter);
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

        public Task<List<T>> ManyAsync(Func<IMongoQueryable<T>, IMongoQueryable<T>> applyFilter)
        {
            return applyFilter(_collection.AsQueryable()).ToListAsync();
        }

        public Task<T> FirstAsync(Func<IMongoQueryable<T>, IMongoQueryable<T>> applyFilter)
        {
            return applyFilter(_collection.AsQueryable()).FirstAsync();
        }

        public Task<T> FirstOrDefaultAsync(Func<IMongoQueryable<T>, IMongoQueryable<T>> applyFilter)
        {
            return applyFilter(_collection.AsQueryable()).FirstOrDefaultAsync();
        }

        public Task UpsertAsync(Expression<Func<T, bool>> predicate, T document)
        {
            return _collection.ReplaceOneAsync(predicate, document, new ReplaceOptions { IsUpsert = true });
        }
    }
}
