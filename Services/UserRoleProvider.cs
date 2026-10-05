using System;
using System.Threading.Tasks;
using Common.Caching;
using Common.Interfaces;

namespace Services;

/// <summary>Resolves a user's role: cache first, database on a miss.</summary>
internal sealed class UserRoleProvider : IUserRoleProvider {
    private readonly IUserRoleCacheService _cache;
    private readonly IUsersManager _manager;

    public UserRoleProvider(
        IUserRoleCacheService cache,
        IUsersManager manager) {
        _cache = cache;
        _manager = manager;
    }

    public async Task<byte?> GetAsync(Guid userPK) {
        var cached = await _cache.GetAsync(userPK);
        if (cached is not null) {
            return cached.RolePK;
        }

        var rolePK = await _manager.GetRoleByPKAsync(userPK);
        if (rolePK is { } value) {
            await _cache.SetAsync(userPK, new CachedUserRole(value));
        }

        return rolePK;
    }
}
