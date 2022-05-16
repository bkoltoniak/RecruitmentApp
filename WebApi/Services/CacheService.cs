using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using RecruitmentApp.Core.Interfaces;
using RecruitmentApp.Core.Options;

namespace RecruitmentApp.WebApi.Services;

public class CacheService : IChacheService
{
    private readonly IMemoryCache _cache;

    private readonly IOptions<CacheOptions> _cacheOptions;

    private readonly MemoryCacheEntryOptions _cacheEntryOptions;

    public CacheService(IMemoryCache cache, IOptions<CacheOptions> cacheOptions)
    {
        _cache = cache;
        _cacheOptions = cacheOptions;

        _cacheEntryOptions = new();
        _cacheEntryOptions
            .SetSlidingExpiration(TimeSpan.FromSeconds(_cacheOptions.Value.SlidingExprationSeconds));
        _cacheEntryOptions
            .SetAbsoluteExpiration(TimeSpan.FromSeconds(_cacheOptions.Value.AbsoluteExprationSeconds));
    }

    public TEntry? GetValueOrDefault<TEntry>(object key)
    {
        TEntry? result = default(TEntry);

        if (_cache.TryGetValue(key, out var entry))
        {
            try
            {
                result = (TEntry)entry;
            }
            catch (InvalidCastException)
            {
            }
        }

        return result;
    }

    public void Set(object key, object value)
    {
        _cache.Set(key, value, _cacheEntryOptions);
    }
}
