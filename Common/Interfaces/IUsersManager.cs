using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Entities;

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
        /// Retrieve a paginated list of users asynchronously.
        /// Returns items and total rows.
        /// </summary>
        /// <param name="requestingRolePK">
        /// The role primary key of the requesting user.
        /// </param>
        /// <param name="currentPage">The current page number.</param>
        /// <param name="pageSize">The number of items per page.</param>
        /// <param name="sortExpression">The expression to sort the results.</param>
        /// <param name="searchValue">The value to search for.</param>
        /// <param name="searchByUserName">Indicates whether to search by user name.</param>
        /// <param name="searchByEmail">Indicates whether to search by email.</param>
        /// <param name="searchByFirstName">Indicates whether to search by first name.</param>
        /// <param name="searchBySecondName">Indicates whether to search by second name.</param>
        /// <param name="includeInactive">Indicates whether to include inactive users.</param>
        /// <param name="strictMatch">Indicates whether to use strict matching.</param>
        Task<(List<User> Items, int TotalRows)> GetByPageAsync(
            byte requestingRolePK,
            int currentPage,
            int pageSize,
            string? sortExpression,
            string? searchValue,
            bool searchByUserName,
            bool searchByEmail,
            bool searchByFirstName,
            bool searchBySecondName,
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