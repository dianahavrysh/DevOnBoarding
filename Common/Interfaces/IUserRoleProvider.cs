using System;
using System.Threading.Tasks;
using Common.Enums;

namespace Common.Interfaces;
/// <summary>
/// Provides a user's role for authorization purposes, using a cache-first approach with database fallback.
/// </summary>
public interface IUserRoleProvider {
    /// <summary>
    /// Returns the role of an active user (cache first, then database),
    /// or null if the user does not exist, is inactive or has an unknown role.
    /// </summary>
    Task<Role?> GetRoleAsync(Guid userPK);
}
