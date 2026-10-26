using System;
using System.Threading;
using System.Threading.Tasks;
using Common.Caching;
using Common.Enums;
using Common.Extensions;
using Common.Interfaces;

namespace Services;

/// <summary>Resolves a user's role: cache first, database on a miss.</summary>
internal sealed class UserRoleProvider : IUserRoleProvider {
    private readonly IUserRoleCacheService _cache;
    private readonly IUsersDbManager _manager;

    public UserRoleProvider(
        IUserRoleCacheService cache,
        IUsersDbManager manager) {
        _cache = cache;
        _manager = manager;
    }
    /// <inheritdoc />
    public async Task<Role?> GetRoleAsync(Guid userPK, CancellationToken cancellationToken = default) {
        var cached = await _cache.GetAsync(userPK, cancellationToken);
        if (cached is not null && RoleExtensions.TryFromPK(cached.RolePK, out var cachedRole)) {
            return cachedRole;
        }

        var rolePK = await _manager.GetRoleByPKAsync(userPK);
        if (rolePK is not { } value || !RoleExtensions.TryFromPK(value, out var role)) {
            return null;
        }

        await _cache.SetAsync(userPK, new CachedUserRole(value), cancellationToken);
        return role;
    }
}
