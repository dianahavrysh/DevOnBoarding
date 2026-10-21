using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Common.Caching;
using Common.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Services.Caching;

/// <summary>
/// Distributed cache implementation for user roles using IDistributedCache (e.g., Redis).
/// Cache failures are logged and treated as cache misses; they never propagate.
/// </summary>
internal sealed class DistributedUserRoleCacheService : IUserRoleCacheService {
    private readonly IDistributedCache _cache;
    private readonly CacheOptions _options;
    private readonly ILogger<DistributedUserRoleCacheService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DistributedUserRoleCacheService"/> class.
    /// </summary>
    /// <param name="cache">The distributed cache instance.</param>
    /// <param name="options">Cache configuration options.</param>
    /// <param name="logger">The logger instance.</param>
    public DistributedUserRoleCacheService(
        IDistributedCache cache,
        IOptions<CacheOptions> options,
        ILogger<DistributedUserRoleCacheService> logger) {
        _cache = cache;
        _options = options.Value;
        _logger = logger;
    }

    
    /// <summary>
    /// Gets the cached role for a user. If the cache read fails, it is logged and treated as a miss.
    /// </summary>
    /// <param name="userPK">The primary key of the user.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The cached role, or null if not found or if cache is unavailable.</returns>
    public async Task<CachedUserRole?> GetAsync(Guid userPK, CancellationToken cancellationToken = default) {
        try {
            var json = await _cache.GetStringAsync(Key(userPK), cancellationToken);

            return json is null
                ? null
                : JsonSerializer.Deserialize<CachedUserRole>(json);
        }
        catch (Exception ex) {
            _logger.LogWarning(ex, "Cache read failed for user {UserPK}; treating as miss.", userPK);
            return null;
        }
    }

    /// <summary>
    /// Sets the cached role for a user. If the cache write fails, it is logged and completes silently.
    /// </summary>
    /// <param name="userPK">The primary key of the user.</param>
    /// <param name="role">The role to cache.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    public async Task SetAsync(Guid userPK, CachedUserRole role, CancellationToken cancellationToken = default) {
        try {
            await _cache.SetStringAsync(
                Key(userPK),
                JsonSerializer.Serialize(role),
                new DistributedCacheEntryOptions {
                    AbsoluteExpirationRelativeToNow = _options.UserRoleTtl
                },
                cancellationToken);
        }
        catch (Exception ex) {
            _logger.LogWarning(ex, "Cache write failed for user {UserPK}.", userPK);
        }
    }

    /// <summary>
    /// Invalidates the cached role for a user. If the cache removal fails, it is logged and completes silently.
    /// </summary>
    /// <param name="userPK">The primary key of the user.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    public async Task InvalidateAsync(Guid userPK, CancellationToken cancellationToken = default) {
        try {
            await _cache.RemoveAsync(Key(userPK), cancellationToken);
        }
        catch (Exception ex) {
            _logger.LogWarning(ex, "Cache invalidation failed for user {UserPK}.", userPK);
        }
    }

    private string Key(Guid userPK) => $"{_options.KeyPrefix}{userPK}";
}
