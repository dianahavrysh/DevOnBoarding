using System;
using System.Text.Json;
using System.Threading.Tasks;
using Common.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace Common.Caching;

/// <summary>
/// Redis-backed implementation of IUserRoleCacheService with graceful fallback on errors.
/// If Redis is unavailable for any reason, operations fail silently (logged as warnings).
/// </summary>
public class RedisUserRoleCacheService : IUserRoleCacheService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<RedisUserRoleCacheService> _logger;

    /// <summary>
    /// Cache key prefix for user roles.
    /// </summary>
    private const string CacheKeyPrefix = "user:role:";

    /// <summary>
    /// Absolute expiration time for cached roles (15 minutes).
    /// </summary>
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(15);

    /// <summary>
    /// Initializes a new instance of the <see cref="RedisUserRoleCacheService"/> class.
    /// </summary>
    public RedisUserRoleCacheService(
        IDistributedCache cache,
        ILogger<RedisUserRoleCacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<CachedUserRole?> GetAsync(Guid userPK)
    {
        try
        {
            var cacheKey = FormatCacheKey(userPK);
            var cachedJson = await _cache.GetStringAsync(cacheKey);

            if (cachedJson == null)
            {
                return null;
            }

            var cachedRole = JsonSerializer.Deserialize<CachedUserRole>(cachedJson);
            _logger.LogDebug("Cache hit for user {UserId}", userPK);
            return cachedRole;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Error retrieving cached role for user {UserId}. Treating as cache miss.",
                userPK);
            return null;
        }
    }

    /// <inheritdoc />
    public async Task SetAsync(Guid userPK, CachedUserRole role)
    {
        try
        {
            var cacheKey = FormatCacheKey(userPK);
            var json = JsonSerializer.Serialize(role);

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = CacheExpiration
            };

            await _cache.SetStringAsync(cacheKey, json, options);
            _logger.LogDebug("Cached role for user {UserId}: {RoleName}", userPK, role.RoleName);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Error storing cached role for user {UserId}. Cache operation skipped; database fallback will be used.",
                userPK);
        }
    }

    /// <inheritdoc />
    public async Task InvalidateAsync(Guid userPK)
    {
        try
        {
            var cacheKey = FormatCacheKey(userPK);
            await _cache.RemoveAsync(cacheKey);
            _logger.LogDebug("Invalidated cache for user {UserId}", userPK);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Error invalidating cached role for user {UserId}. Cache operation skipped; new cache will be populated on next read.",
                userPK);
        }
    }

    /// <summary>
    /// Formats the cache key for a user role.
    /// </summary>
    private static string FormatCacheKey(Guid userPK) =>
        $"{CacheKeyPrefix}{userPK}";
}
