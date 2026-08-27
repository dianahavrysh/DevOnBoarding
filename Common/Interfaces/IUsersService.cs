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
        /// Retrieve a paginated list of user DTOs asynchronously.
        /// </summary>
        Task<(IEnumerable<UserDTO> Items, int TotalRows)> GetByPageAsync(
            Guid requestingUserPK,
            int currentPage,
            int pageSize,
            string? sortExpression,
            string? searchValue,
            Dictionary<string, bool>? searchByFields,
            bool includeInactive,
            bool strictMatch);

        /// <summary>
        /// Create a new user from the supplied DTO.
        /// The generated primary key is stored in the DTO.
        /// </summary>
        Task<UserCreateUpdateDTO> CreateAsync(UserCreateUpdateDTO dto);

        /// <summary>
        /// Update an existing user using the primary key stored in the DTO.
        /// Returns true if the user exists and was updated.
        /// </summary>
        Task<bool> UpdateAsync(UserCreateUpdateDTO dto);

        /// <summary>
        /// Delete the user identified by the specified primary key asynchronously.
        /// </summary>
        Task DeleteAsync(Guid userPK);
    }
}
