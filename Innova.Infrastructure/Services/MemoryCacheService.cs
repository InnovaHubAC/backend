using System.Collections.Concurrent;

namespace Innova.Infrastructure.Services;

public class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly ConcurrentDictionary<string, byte> _cacheKeys;
    public MemoryCacheService(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
        _cacheKeys = new ConcurrentDictionary<string, byte>();
    }

    public T? Get<T>(string key) where T : class
    {
        _memoryCache.TryGetValue(key, out T? cacheEntry);
        return cacheEntry ?? null;
    }

    public void Remove(string key)
    {
        _memoryCache.Remove(key);
        _cacheKeys.TryRemove(key, out _);
    }

    public void RemoveByPrefix(string prefix)
    {
        var keysToRemove = _cacheKeys.Keys
           .Where(k => k.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
           .ToList();

        foreach (var key in keysToRemove)
        {
            _memoryCache.Remove(key);
            _cacheKeys.TryRemove(key, out _);
        }
    }

    public void Set<T>(string key, T value, MemoryCacheEntryOptions options) where T : class
    {
        _cacheKeys.TryAdd(key, 0);
        // callback to remove from tracking when expired.
        options.RegisterPostEvictionCallback((k, v, r, s) =>
        {
            _cacheKeys.TryRemove(k.ToString()!, out _);
        });
        _memoryCache.Set(key, value, options);
    }

    public MemoryCacheEntryOptions SetMemoryCacheEntryOptions(TimeSpan absoluteExpiration)
    {
        return new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = absoluteExpiration
        };
    }

    public MemoryCacheEntryOptions SetMemoryCacheEntryOptions(TimeSpan absoluteExpiration, TimeSpan slidingExpiration)
    {
        return new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = absoluteExpiration,
            SlidingExpiration = slidingExpiration
        };
    }
}
