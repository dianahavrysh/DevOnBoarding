using System;
using System.Text.Json;
using System.Threading.Tasks;
using Common.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace Common.Caching;

/// <summary>
/// Redis-backed role cache. Redis failures never propagate: they are logged and treated as a miss.
/// </summary>
public class RedisUserRoleCacheService : IUserRoleCacheService {
    private const string KeyPrefix = "user:role:";
    private static readonly TimeSpan Expiration = TimeSpan.FromMinutes(15);

    private readonly IDistributedCache _cache;
    private readonly ILogger<RedisUserRoleCacheService> _logger;

    public RedisUserRoleCacheService(
        IDistributedCache cache,
        ILogger<RedisUserRoleCacheService> logger) {
        _cache = cache;
        _logger = logger;
    }

    public async Task<CachedUserRole?> GetAsync(Guid userPK) {
        try {
            var json = await _cache.GetStringAsync(Key(userPK));

            return json is null
                ? null
                : JsonSerializer.Deserialize<CachedUserRole>(json);
        }
        catch (Exception ex) {
            _logger.LogWarning(ex, "Cache read failed for user {UserPK}; treating as miss.", userPK);
            return null;
        }
    }

    public async Task SetAsync(Guid userPK, CachedUserRole role) {
        try {
            await _cache.SetStringAsync(
                Key(userPK),
                JsonSerializer.Serialize(role),
                new DistributedCacheEntryOptions {
                    AbsoluteExpirationRelativeToNow = Expiration
                });
        }
        catch (Exception ex) {
            _logger.LogWarning(ex, "Cache write failed for user {UserPK}.", userPK);
        }
    }

    public async Task InvalidateAsync(Guid userPK) {
        try {
            await _cache.RemoveAsync(Key(userPK));
        }
        catch (Exception ex) {
            _logger.LogWarning(ex, "Cache invalidation failed for user {UserPK}.", userPK);
        }
    }

    private static string Key(Guid userPK) => $"{KeyPrefix}{userPK}";
}
