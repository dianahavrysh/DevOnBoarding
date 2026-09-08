using Common.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Common.Interfaces
{
    /// <summary>
    /// Manager interface for user operations.
    /// </summary>
    public interface IUsersManager
    {
        /// <summary>
        /// Get a user entity by primary key asynchronously.
        /// </summary>
        Task<User?> GetByPKAsync(Guid userPK);

        /// <summary>
        /// Get a user entity by email asynchronously.
        /// Uses a dedicated stored procedure for efficient lookup.
        /// </summary>
        /// <param name="email">The email to search for.</param>
        /// <returns>The user if found; otherwise null.</returns>
        Task<User?> GetByEmailAsync(string email);

        /// <summary>
        /// Retrieve a paginated list of users asynchronously. Returns items and total rows.
        /// </summary>
        Task<(List<User> Items, int TotalRows)> GetByPageAsync(
            Guid requestingUserPK, 
            int currentPage, 
            int pageSize, 
            string? sortExpression, 
            string? searchValue, 
            Dictionary<string, bool>? searchByFields, 
            bool includeInactive, 
            bool strictMatch);

        /// <summary>
        /// Insert a user asynchronously and return the created primary key.
        /// </summary>
        Task<Guid> InsertAsync(User user);

        /// <summary>
        /// Update a user asynchronously.
        /// </summary>
        Task<bool> UpdateAsync(User user);

        /// <summary>
        /// Delete a user by primary key asynchronously.
        /// </summary>
        Task DeleteAsync(Guid userPK);
    }
}
