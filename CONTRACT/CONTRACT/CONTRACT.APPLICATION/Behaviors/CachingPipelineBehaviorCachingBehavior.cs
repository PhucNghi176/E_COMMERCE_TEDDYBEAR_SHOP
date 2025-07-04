using CONTRACT.CONTRACT.APPLICATION.Abstractions;
using CONTRACT.CONTRACT.CONTRACT.Abstractions.Shared;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;

namespace CONTRACT.CONTRACT.APPLICATION.Behaviors;
public class CachingPipelineBehaviorCachingBehavior<TRequest, TResponse>(
    ICacheService cacheService
)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICacheable
    where TResponse : Result
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        TResponse response;
        // By Pass Cache
        if (request.BypassCache) return await next(cancellationToken);

        var cachedResponse = await cacheService.GetAsync<TResponse>(request.CacheKey, cancellationToken);

        // Take In Cache
        // Exist => Return
        // Not Exist => Persist, UpdateCache
        if (cachedResponse != null)
            response = cachedResponse;
        // logger.LogInformation("fetched from cache with key : {CacheKey}", request.CacheKey);
        else
            response = await GetResponseAndAddToCache();
        // logger.LogInformation("added to cache with key : {CacheKey}", request.CacheKey);
        return response;

        async Task<TResponse> GetResponseAndAddToCache()
        {
            response = await next(cancellationToken);
            var slidingExpiration =
                request.SlidingExpirationInMinutes == 0 ? 30 : request.SlidingExpirationInMinutes;
            var absoluteExpiration =
                request.AbsoluteExpirationInMinutes == 0 ? 60 : request.AbsoluteExpirationInMinutes;
            var options = new DistributedCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(slidingExpiration))
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(absoluteExpiration));

            await cacheService.SetAsync(request.CacheKey, response, options, cancellationToken);

            return response;
        }
    }
}