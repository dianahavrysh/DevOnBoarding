using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Common.DTOs;
using Common.Entities;
using Common.Interfaces;

namespace Services;

/// <summary>
/// Application service exposing user operations as DTOs.
/// Keeps the role cache consistent with changes made to users.
/// </summary>
public class UsersService : IUsersService {
    private readonly IUsersManager _manager;
    private readonly IUserRoleCacheService _roleCache;
    private readonly ICurrentUserContext _currentUser;
    private readonly IMapper _mapper;

    public UsersService(
        IUsersManager manager,
        IUserRoleCacheService roleCache,
        ICurrentUserContext currentUser,
        IMapper mapper) {
        _manager = manager;
        _roleCache = roleCache;
        _currentUser = currentUser;
        _mapper = mapper;
    }

    /// <inheritdoc />
    public async Task<UserDTO?> GetByPKAsync(Guid userPK) =>
        _mapper.Map<UserDTO?>(await _manager.GetByPKAsync(userPK));

    /// <inheritdoc />
    public async Task<(List<UserDTO> Items, int TotalRows)> GetByPageAsync(
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
        var (items, totalRows) = await _manager.GetByPageAsync(
            (byte)_currentUser.Role,
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

        return (_mapper.Map<List<UserDTO>>(items), totalRows);
    }

    /// <inheritdoc />
    public async Task<UserDTO> CreateAsync(UserCreateUpdateDTO dto) {
        var userPK = await _manager.InsertAsync(_mapper.Map<User>(dto));
        var created = await _manager.GetByPKAsync(userPK);

        return _mapper.Map<UserDTO>(created);
    }

    /// <inheritdoc />
    public async Task<bool> UpdateAsync(UserCreateUpdateDTO dto) {
        var updated = await _manager.UpdateAsync(_mapper.Map<User>(dto));

        if (updated) {
            await _roleCache.InvalidateAsync(dto.UserPK);
        }

        return updated;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(Guid userPK) {
        var deleted = await _manager.DeleteAsync(userPK);
        await _roleCache.InvalidateAsync(userPK);

        return deleted;
    }
}
