using Common.DTOs;
using Common.Entities;
using System;
using System.Collections.Generic;

namespace Common.Interfaces
{
    /// <summary>
    /// Service interface producing DTOs for client consumption.
    /// </summary>
    public interface IUsersService
    {
        /// <summary>
        /// Get a user DTO by primary key asynchronously.
        /// </summary>
        Task<UserDTO?> GetByPKAsync(Guid userPK);

        /// <summary>
        /// Retrieve a paginated list of user DTOs asynchronously.
        /// Returns items and total rows.
        /// </summary>
        Task<(IEnumerable<UserDTO> Items, int TotalRows)> GetByPageAsync(Guid requestingUserPK, int currentPage, int pageSize, string? sortExpression, string? searchValue, Dictionary<string, bool>? searchByFields, bool includeInactive, bool strictMatch);

        /// <summary>
        /// Create a new user from the supplied DTO asynchronously.
        /// </summary>
        Task<Guid> CreateAsync(UserCreateUpdateDTO dto);

        /// <summary>
        /// Update an existing user asynchronously. Returns true if updated, false if not found.
        /// </summary>
        Task<bool> UpdateAsync(Guid userPK, UserCreateUpdateDTO dto);

        /// <summary>
        /// Delete the user identified by the specified primary key asynchronously.
        /// </summary>
        Task DeleteAsync(Guid userPK);
    }
}
