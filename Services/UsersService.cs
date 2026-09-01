using AutoMapper;
using Common.DTOs;
using Common.Entities;
using Common.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Services {
    /// <summary>
    /// Application service that exposes user-related operations using DTOs for client consumption.
    /// </summary>
    public class UsersService : IUsersService {
        private readonly IUsersManager _manager;
        private readonly ILogger<UsersService> _logger;
        private readonly IMapper _mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="UsersService"/> class.
        /// </summary>
        public UsersService(
            IUsersManager manager,
            ILogger<UsersService> logger,
            IMapper mapper) {
            _manager = manager;
            _logger = logger;
            _mapper = mapper;
        }

        /// <inheritdoc />
        public async Task<UserDTO> CreateAsync(UserCreateUpdateDTO dto) {
            try {
                var user = _mapper.Map<User>(dto);
                var userPK = await _manager.InsertAsync(user);

                user.UserPK = userPK;

                return _mapper.Map<UserDTO>(user);
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Error creating user");
                throw;
            }
        }

        /// <inheritdoc />
        public async Task DeleteAsync(Guid userPK) {
            try {
                await _manager.DeleteAsync(userPK);
            }
            catch (Exception ex) {
                _logger.LogError(
                    ex,
                    "Error deleting user {UserPK}",
                    userPK);

                throw;
            }
        }

        /// <inheritdoc />
        public async Task<UserDTO?> GetByPKAsync(Guid userPK) {
            try {
                var user = await _manager.GetByPKAsync(userPK);

                return _mapper.Map<UserDTO>(user);
            }
            catch (Exception ex) {
                _logger.LogError(
                    ex,
                    "Error getting user by PK {UserPK}",
                    userPK);

                throw;
            }
        }

        /// <inheritdoc />
        public async Task<(List<UserDTO> Items, int TotalRows)> GetByPageAsync(
            Guid requestingUserPK,
            int currentPage,
            int pageSize,
            string? sortExpression,
            string? searchValue,
            Dictionary<string, bool>? searchByFields,
            bool includeInactive,
            bool strictMatch) {
            try {
                var (items, total) = await _manager.GetByPageAsync(
                    requestingUserPK,
                    currentPage,
                    pageSize,
                    sortExpression,
                    searchValue,
                    searchByFields,
                    includeInactive,
                    strictMatch);

                return (
                    _mapper.Map<List<UserDTO>>(items),
                    total);
            }
            catch (Exception ex) {
                _logger.LogError(
                    ex,
                    "Error getting users page " +
                    "(RequestingUserPK={RequestingUserPK}, " +
                    "Page={CurrentPage}, PageSize={PageSize})",
                    requestingUserPK,
                    currentPage,
                    pageSize);

                throw;
            }
        }

        /// <inheritdoc />
        public async Task<bool> UpdateAsync(UserCreateUpdateDTO dto) {
            try {
                var user = _mapper.Map<User>(dto);

                return await _manager.UpdateAsync(user);
            }
            catch (Exception ex) {
                _logger.LogError(
                    ex,
                    "Error updating user {UserPK}",
                    dto.UserPK);

                throw;
            }
        }
    }
}
