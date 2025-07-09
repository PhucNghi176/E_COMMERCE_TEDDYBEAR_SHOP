using System.Collections.Concurrent;
using CONTRACT.CONTRACT.APPLICATION.Abstractions;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

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
    
    // Optimize JSON serialization by reusing settings
    private static readonly JsonSerializerSettings SerializerSettings = new()
    {
        ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
        ContractResolver = new DefaultContractResolver
        {
            NamingStrategy = new CamelCaseNamingStrategy()
        }
    };

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        where T : class
    {
        var cacheValue = await distributedCache.GetStringAsync(key, cancellationToken);

        if (cacheValue is null)
            return null;
            
        var value = JsonConvert.DeserializeObject<T>(cacheValue, SerializerSettings);
        return value;
    }

    public async Task SetAsync<T>(string key, T value, DistributedCacheEntryOptions? options,
        CancellationToken cancellationToken = default) where T : class
    {
        var cacheValue = JsonConvert.SerializeObject(value, SerializerSettings);

        // Fix: Only call SetStringAsync once, with options if provided
        if (options != null)
        {
            await distributedCache.SetStringAsync(key, cacheValue, options, cancellationToken);
        }
        else
        {
            await distributedCache.SetStringAsync(key, cacheValue, cancellationToken);
        }

        _ = CacheKeys.TryAdd(key, false);
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        await distributedCache.RemoveAsync(key, cancellationToken);
        _ = CacheKeys.TryRemove(key, out _);
    }

    public async Task RemoveByPrefixAsync(string prefixKey, CancellationToken cancellationToken = default)
    {
        // Optimize: Use parallel execution with proper exception handling
        var matchingKeys = CacheKeys.Keys.Where(k => k.StartsWith(prefixKey)).ToList();
        
        if (matchingKeys.Count == 0) return;
        
        var tasks = matchingKeys.Select(async k =>
        {
            try
            {
                await RemoveAsync(k, cancellationToken);
            }
            catch (Exception)
            {
                // Log if needed, but don't fail the entire operation
            }
        });

        await Task.WhenAll(tasks);
    }
}