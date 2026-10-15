using Common.DTOs;
using Common.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Common.Interfaces {
    /// <summary>
    /// Service interface producing DTOs for client consumption.
    /// Authorizes every operation against the current user.
    /// </summary>
    public interface IUsersService {
        /// <summary>
        /// Get a user DTO by primary key asynchronously.
        /// </summary>
        Task<OperationResult<UserDTO>> GetByPKAsync(Guid userPK);

        /// <summary>
        /// Retrieve a paginated list of user DTOs asynchronously. Returns items and total rows.
        /// </summary>
        Task<(List<UserDTO> Items, int TotalRows)> GetByPageAsync(
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
        Task<OperationResult<UserDTO>> CreateAsync(UserCreateUpdateDTO dto);

        /// <summary>
        /// Update an existing user using the primary key stored in the DTO.
        /// Returns the operation status.
        /// </summary>
        Task<OperationStatus> UpdateAsync(UserCreateUpdateDTO dto);

        /// <summary>
        /// Delete the user identified by the specified primary key asynchronously.
        /// Returns the operation status.
        /// </summary>
        Task<OperationStatus> DeleteAsync(Guid userPK);
    }
}
