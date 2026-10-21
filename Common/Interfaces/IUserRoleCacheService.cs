using System;
using System.Threading;
using System.Threading.Tasks;
using Common.Caching;

namespace Common.Interfaces;

/// <summary>
/// Abstraction for caching user roles. The distributed cache being unavailable must not break authorization.
/// </summary>
public interface IUserRoleCacheService
{
    /// <summary>
    /// Attempt to retrieve a cached user role by user primary key.
    /// Returns null on cache miss or on any error. Never throws.
    /// </summary>
    /// <param name="userPK">User primary key</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>Cached role, or null if not found or if cache is unavailable</returns>
    Task<CachedUserRole?> GetAsync(Guid userPK, CancellationToken cancellationToken = default);

    /// <summary>
    /// Store a user role in the cache. If the cache is unavailable, logs a warning and completes silently.
    /// Never throws.
    /// </summary>
    /// <param name="userPK">User primary key</param>
    /// <param name="role">Role to cache</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    Task SetAsync(Guid userPK, CachedUserRole role, CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalidate the cached role for a specific user.
    /// If the cache is unavailable, logs a warning and completes silently.
    /// Never throws.
    /// </summary>
    /// <param name="userPK">User primary key</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    Task InvalidateAsync(Guid userPK, CancellationToken cancellationToken = default);
}
