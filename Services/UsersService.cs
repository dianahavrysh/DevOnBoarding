using Common.DTOs;
using Common.Entities;
using Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using AutoMapper;

namespace Services
{
    /// <summary>
    /// Application service that exposes user-related operations using DTOs for client consumption.
    /// Maps between domain entities and DTOs and delegates persistence to the users manager.
    /// </summary>
    public class UsersService : IUsersService
    {
        private readonly IUsersManager _manager;
        private readonly ILogger<UsersService> _logger;
        private readonly IMapper _mapper;
        /// <summary>
        /// Initializes a new instance of <see cref="UsersService"/>.
        /// </summary>
        /// <param name="manager">Business logic manager used for persistence and retrieval.</param>
        /// <param name="logger">Logger instance for this service.</param>
        public UsersService(IUsersManager manager, ILogger<UsersService> logger, IMapper mapper)
        {
            _manager = manager;
            _logger = logger;
            _mapper = mapper;
        }

        /// <summary>
        /// Create a new user from the supplied DTO.
        /// </summary>
        /// <param name="dto">Write DTO containing user fields to create.</param>
        /// <returns>The primary key of the created user.</returns>
        public async Task<Guid> CreateAsync(UserCreateUpdateDTO dto)
        {
            var user = _mapper.Map<User>(dto);
            try
            {
                return await _manager.InsertAsync(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                throw;
            }
        }

        /// <summary>
        /// Delete the user identified by the specified primary key.
        /// </summary>
        /// <param name="userPK">Primary key of the user to delete.</param>
        public async Task DeleteAsync(Guid userPK)
        {
            try
            {
                await _manager.DeleteAsync(userPK);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {UserPK}", userPK);
                throw;
            }
        }

        /// <summary>
        /// Get a user DTO by primary key.
        /// </summary>
        /// <param name="userPK">User primary key.</param>
        /// <returns>A <see cref="UserDTO"/> if the user exists; otherwise <c>null</c>.</returns>
        public async Task<UserDTO?> GetByPKAsync(Guid userPK)
        {
            try
            {
                var u = await _manager.GetByPKAsync(userPK);
                if (u == null) return null;
                return _mapper.Map<UserDTO>(u);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user by PK {UserPK}", userPK);
                throw;
            }
        }

        /// <summary>
        /// Retrieve a paged list of user DTOs.
        /// </summary>
        public async Task<(IEnumerable<UserDTO> Items, int TotalRows)> GetByPageAsync(Guid requestingUserPK, int currentPage, int pageSize, string? sortExpression, string? searchValue, Dictionary<string, bool>? searchByFields, bool includeInactive, bool strictMatch)
        {
            try
            {
                var (items, total) = await _manager.GetByPageAsync(requestingUserPK, currentPage, pageSize, sortExpression, searchValue, searchByFields, includeInactive, strictMatch);
                var list = new List<UserDTO>();
                foreach (var u in items)
                    list.Add(_mapper.Map<UserDTO>(u));
                return (list, total);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users page (RequestingUserPK={RequestingUserPK}, Page={CurrentPage}, PageSize={PageSize})", requestingUserPK, currentPage, pageSize);
                throw;
            }
        }

        /// <summary>
        /// Update an existing user with values from the write DTO.
        /// Returns true if updated, false if user not found.
        /// </summary>
        /// <param name="userPK">Primary key of the user to update.</param>
        /// <param name="dto">Write DTO with updated values.</param>
        public async Task<bool> UpdateAsync(Guid userPK, UserCreateUpdateDTO dto)
        {
            var existing = await _manager.GetByPKAsync(userPK);
            if (existing == null) return false;

            _mapper.Map(dto, existing);

            try
            {
                await _manager.UpdateAsync(existing);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserPK}", userPK);
                throw;
            }
        }
    }
}

