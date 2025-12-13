using Microsoft.Extensions.Caching.Memory;

namespace Innova.Domain.Interfaces
{
    public interface ICacheService
    {
        T? Get<T>(string key) where T : class;
        void Remove(string key);
        void Set<T>(string key, T value, MemoryCacheEntryOptions options) where T : class;
        MemoryCacheEntryOptions SetMemoryCacheEntryOptions(TimeSpan absoluteExpiration);
        MemoryCacheEntryOptions SetMemoryCacheEntryOptions(TimeSpan absoluteExpiration,
            TimeSpan slidingExpiration);
        void RemoveByPrefix(string prefix);
    }
}
