using System;

namespace Services.Caching;

/// <summary>
/// Configuration options for the user role cache.
/// </summary>
public class CacheOptions {
    /// <summary>
    /// The time-to-live (TTL) for cached role entries.
    /// Default: 15 minutes.
    /// </summary>
    public TimeSpan UserRoleTtl { get; set; } = TimeSpan.FromMinutes(15);

    /// <summary>
    /// The prefix prepended to all cache keys to avoid collisions.
    /// Default: "user:role:v1:"
    /// </summary>
    public string KeyPrefix { get; set; } = "user:role:v1:";
}
