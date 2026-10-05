using System;
using System.Threading.Tasks;

namespace Common.Interfaces;
/// <summary>
/// Provides a way to retrieve the role id of an active user by their primary key.
/// </summary>
public interface IUserRoleProvider {
    /// <summary>
    /// Returns the role id of an active user (cache first, then database),
    /// or null if the user does not exist or is inactive.
    /// </summary>
    Task<byte?> GetAsync(Guid userPK);
}
