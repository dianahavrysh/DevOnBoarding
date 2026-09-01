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
        /// Retrieve a paginated list of users asynchronously. Returns items and total rows.
        /// </summary>
        Task<(List<User> Items, int TotalRows)> GetByPageAsync(
            Guid requestingUserPK, 
            int currentPage, 
            int pageSize, 
            string? sortExpression, 
            string? searchValue, 
            Dictionary<string,
            bool>? searchByFields, 
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
