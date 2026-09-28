using Common.Caching;
using Common.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Common.Interfaces {
    /// <summary>
    /// Service interface producing DTOs for client consumption.
    /// </summary>
    public interface IUsersService {
        /// <summary>
        /// Get a user DTO by primary key asynchronously.
        /// </summary>
        Task<UserDTO?> GetByPKAsync(Guid userPK);

        /// <summary>
        /// Retrieve a paginated list of user DTOs asynchronously. Returns items and total rows.
        /// </summary>
        Task<(List<UserDTO> Items, int TotalRows)> GetByPageAsync(
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
        /// Create a new user and return the created user DTO.
        /// </summary>
        Task<UserDTO> CreateAsync(UserCreateUpdateDTO dto);

        /// <summary>
        /// Update an existing user using the primary key stored in the DTO.
        /// Returns true if the user exists and was updated.
        /// </summary>
        Task<bool> UpdateAsync(UserCreateUpdateDTO dto);

        /// <summary>
        /// Delete the user identified by the specified primary key asynchronously.
        /// </summary>
        Task DeleteAsync(Guid userPK);

        /// <summary>
        /// Get the cached user role by user primary key, with cache-first logic.
        /// On a cache miss, falls back to the database and populates the cache.
        /// Returns null if the user is not found.
        /// The cache service ensures Redis unavailability never propagates exceptions.
        /// </summary>
        /// <param name="userPK">User primary key</param>
        /// <returns>Cached or database-fetched role, or null if user not found</returns>
        Task<CachedUserRole?> GetRoleAsync(Guid userPK);
    }
}
