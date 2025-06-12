using CONTRACT.CONTRACT.APPLICATION.Abstractions;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections.Concurrent;

namespace CONTRACT.CONTRACT.INFRASTRUCTURE.Caching;
public class CacheService(IDistributedCache distributedCache) : ICacheService
{
    /**
     * Because we don't have any method to get all of the keys in redis
     * => Solution: Store key in memory at set value to redis
     * 
     * =>> Cache Service can be used concurrently, so we have to make sure that the data structure that we choose is thead safe => use ConcurrentDictionary
     */
    private static readonly ConcurrentDictionary<string, bool> CacheKeys = new();

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        where T : class
    {
        var cacheValue = await distributedCache.GetStringAsync(key, cancellationToken);

        if (cacheValue is null)
            return null;
        var value = JsonConvert.DeserializeObject<T>(cacheValue, new JsonSerializerSettings
        {
            ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            }
        });
        return value;
    }

    public async Task SetAsync<T>(string key, T value, DistributedCacheEntryOptions? options,
        CancellationToken cancellationToken = default) where T : class
    {
        var cacheValue = JsonConvert.SerializeObject(value, new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            }
        });

        if (options != null) await distributedCache.SetStringAsync(key, cacheValue, options, cancellationToken);

        await distributedCache.SetStringAsync(key, cacheValue, cancellationToken);

        _ = CacheKeys.TryAdd(key, false);
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        await distributedCache.RemoveAsync(key, cancellationToken);
        _ = CacheKeys.TryRemove(key, out _);
    }

    public async Task RemoveByPrefixAsync(string prefixKey, CancellationToken cancellationToken = default)
    {
        //foreach (string key in CacheKeys.Keys)
        //{
        //    if (key.StartsWith(prefixKey))
        //        await RemoveAsync(key, cancellationToken); // Call remove one by one
        //}

        var tasks = CacheKeys.Keys.Where(k => k.StartsWith(prefixKey))
            .Select(k => RemoveAsync(k, cancellationToken));

        await Task.WhenAll(tasks); // Execute in parallel
    }
}