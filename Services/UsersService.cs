using Common.DTOs;
using Common.Entities;
using Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Services.Mappers;

namespace Services
{
    /// <summary>
    /// Application service that exposes user-related operations using DTOs for client consumption.
    /// Maps between domain entities and DTOs and delegates persistence to the users manager.
    /// </summary>
    public class UsersService : IUsersService
    {
        private readonly IUsersManager _manager;

        /// <summary>
        /// Initializes a new instance of <see cref="UsersService"/>.
        /// </summary>
        /// <param name="manager">Business logic manager used for persistence and retrieval.</param>
        public UsersService(IUsersManager manager)
        {
            _manager = manager;
        }

        /// <summary>
        /// Create a new user from the supplied DTO.
        /// </summary>
        /// <param name="dto">Write DTO containing user fields to create.</param>
        /// <returns>The primary key of the created user.</returns>
        public async Task<Guid> CreateAsync(UserCreateUpdateDTO dto)
        {
            var user = UserMapper.ToEntity(dto);
            return await _manager.InsertAsync(user).ConfigureAwait(false);
        }

        /// <summary>
        /// Delete the user identified by the specified primary key.
        /// </summary>
        /// <param name="userPK">Primary key of the user to delete.</param>
        public async Task DeleteAsync(Guid userPK)
        {
            await _manager.DeleteAsync(userPK).ConfigureAwait(false);
        }

        /// <summary>
        /// Get a user DTO by primary key.
        /// </summary>
        /// <param name="userPK">User primary key.</param>
        /// <returns>A <see cref="UserDTO"/> if the user exists; otherwise <c>null</c>.</returns>
        public async Task<UserDTO?> GetByPKAsync(Guid userPK)
        {
            var u = await _manager.GetByPKAsync(userPK).ConfigureAwait(false);
            if (u == null) return null;
            return UserMapper.ToDto(u);
        }

        /// <summary>
        /// Retrieve a paged list of user DTOs.
        /// </summary>
        public async Task<(IEnumerable<UserDTO> Items, int TotalRows)> GetByPageAsync(Guid requestingUserPK, int currentPage, int pageSize, string? sortExpression, string? searchValue, Dictionary<string, bool>? searchByFields, bool includeInactive, bool strictMatch)
        {
            var (items, total) = await _manager.GetByPageAsync(requestingUserPK, currentPage, pageSize, sortExpression, searchValue, searchByFields, includeInactive, strictMatch).ConfigureAwait(false);
            var list = new List<UserDTO>();
            foreach (var u in items)
                list.Add(UserMapper.ToDto(u));
            return (list, total);
        }

        /// <summary>
        /// Update an existing user with values from the write DTO.
        /// Returns true if updated, false if user not found.
        /// </summary>
        /// <param name="userPK">Primary key of the user to update.</param>
        /// <param name="dto">Write DTO with updated values.</param>
        public async Task<bool> UpdateAsync(Guid userPK, UserCreateUpdateDTO dto)
        {
            var existing = await _manager.GetByPKAsync(userPK).ConfigureAwait(false);
            if (existing == null) return false;

            UserMapper.ApplyUpdate(existing, dto);

            await _manager.UpdateAsync(existing).ConfigureAwait(false);
            return true;
        }
    }
}

