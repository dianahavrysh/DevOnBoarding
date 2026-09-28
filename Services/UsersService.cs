using AutoMapper;
using Common.Caching;
using Common.DTOs;
using Common.Entities;
using Common.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Services;
/// <summary>
/// Application service that exposes user-related operations using DTOs for client consumption.
/// </summary>
public class UsersService : IUsersService {
    private readonly IUsersManager _manager;
    private readonly ILogger<UsersService> _logger;
    private readonly IMapper _mapper;
    private readonly IUserRoleCacheService _cacheService;

    /// <summary>
    /// Initializes a new instance of the <see cref="UsersService"/> class.
    /// </summary>
    public UsersService(
        IUsersManager manager,
        ILogger<UsersService> logger,
        IMapper mapper,
        IUserRoleCacheService cacheService) {
        _manager = manager;
        _logger = logger;
        _mapper = mapper;
        _cacheService = cacheService;
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

            await _cacheService.InvalidateAsync(userPK);
            _logger.LogInformation("Cache invalidated for deleted user {UserPK}", userPK);
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
        bool strictMatch) {
        try {
            var (items, total) = await _manager.GetByPageAsync(
                requestingRolePK,
                currentPage,
                pageSize,
                sortExpression,
                searchValue,
                searchByUserName,
                searchByEmail,
                searchByFirstName,
                searchBySecondName,
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
                "(RequestingRolePK={RequestingRolePK}, " +
                "Page={CurrentPage}, PageSize={PageSize})",
                requestingRolePK,
                currentPage,
                pageSize);

            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> UpdateAsync(UserCreateUpdateDTO dto) {
        try {
            var existingUser = await _manager.GetByPKAsync(dto.UserPK);
            var existingRolePK = existingUser?.RolePK ?? 0;

            var user = _mapper.Map<User>(dto);
            var updateSucceeded = await _manager.UpdateAsync(user);

            if (updateSucceeded && user.RolePK != existingRolePK) {
                await _cacheService.InvalidateAsync(dto.UserPK);
                _logger.LogInformation(
                    "Role changed for user {UserPK} (old: {OldRolePK}, new: {NewRolePK}). Cache invalidated.",
                    dto.UserPK,
                    existingRolePK,
                    user.RolePK);
            }

            return updateSucceeded;
        }
        catch (Exception ex) {
            _logger.LogError(
                ex,
                "Error updating user {UserPK}",
                dto.UserPK);

            throw;
        }
    }

    /// <inheritdoc />
    public async Task<CachedUserRole?> GetRoleAsync(Guid userPK) {
        try {
            var cachedRole = await _cacheService.GetAsync(userPK);
            if (cachedRole != null) {
                return cachedRole;
            }

            var user = await _manager.GetByPKAsync(userPK);
            if (user == null) {
                return null;
            }

            var role = new CachedUserRole(user.RolePK, user.RoleName);

            await _cacheService.SetAsync(userPK, role);

            return role;
        }
        catch (Exception ex) {
            _logger.LogError(
                ex,
                "Error retrieving role for user {UserPK}",
                userPK);
            throw;
        }
    }
}
