using System.Threading.Tasks;

namespace Noppes.Fluffle.Database.KeyValue
{
    public interface IKeyValueStore
    {
        Task<KeyValueResult<T>> GetAsync<T>(string key);

        Task SetAsync<T>(string key, T value);
    }
}
