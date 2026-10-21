using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Common.DTOs;
using Common.Entities;
using Common.Enums;
using Common.Interfaces;

namespace Services;

/// <summary>
/// Application service exposing user operations as DTOs.
/// Authorizes every operation against the current user and keeps the role cache consistent.
/// </summary>
public class UsersService : IUsersService {
    private readonly IUsersDbManager _manager;
    private readonly IUserRoleCacheService _roleCache;
    private readonly ICurrentUserContext _currentUser;
    private readonly IUserAccessChecker _access;
    private readonly IMapper _mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="UsersService"/> class.
    /// </summary>
    /// <param name="manager">The users manager for data access operations.</param>
    /// <param name="roleCache">The user role cache service for invalidating cached roles.</param>
    /// <param name="currentUser">The current user context providing role information.</param>
    /// <param name="access">The user access checker for authorization operations.</param>
    /// <param name="mapper">The AutoMapper instance for DTO mapping.</param>
    public UsersService(
        IUsersDbManager manager,
        IUserRoleCacheService roleCache,
        ICurrentUserContext currentUser,
        IUserAccessChecker access,
        IMapper mapper) {
        _manager = manager;
        _roleCache = roleCache;
        _currentUser = currentUser;
        _access = access;
        _mapper = mapper;
    }

    /// <inheritdoc />
    public async Task<OperationResult<UserDTO>> GetByPKAsync(Guid userPK) {
        var user = await _manager.GetByPKAsync(userPK);
        if (user is null) {
            return OperationResult<UserDTO>.NotFound();
        }

        if (!_access.IsAllowed(ToTarget(user), UserOperation.View)) {
            return OperationResult<UserDTO>.Forbidden();
        }

        return OperationResult<UserDTO>.Success(_mapper.Map<UserDTO>(user));
    }

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
    public async Task<OperationResult<UserDTO>> CreateAsync(UserCreateUpdateDTO dto) {
        var target = new TargetUserInfo(Guid.Empty, dto.RolePK);
        if (!_access.IsAllowed(target, UserOperation.Create)) {
            return OperationResult<UserDTO>.Forbidden();
        }

        var userPK = await _manager.InsertAsync(_mapper.Map<User>(dto));
        var created = await _manager.GetByPKAsync(userPK);

        return created is null
            ? OperationResult<UserDTO>.NotFound()
            : OperationResult<UserDTO>.Success(_mapper.Map<UserDTO>(created));
    }

    /// <inheritdoc />
    public async Task<OperationStatus> UpdateAsync(UserCreateUpdateDTO dto) {
        var existing = await _manager.GetByPKAsync(dto.UserPK);
        if (existing is null) {
            return OperationStatus.NotFound;
        }

        var target = ToTarget(existing);
        if (!_access.IsAllowed(target, UserOperation.Edit)) {
            return OperationStatus.Forbidden;
        }

        if (dto.RolePK != existing.RolePK
            && !_access.IsAllowed(target with { RolePK = dto.RolePK }, UserOperation.AssignRole)) {
            return OperationStatus.Forbidden;
        }

        var updated = await _manager.UpdateAsync(_mapper.Map<User>(dto));
        if (!updated) {
            return OperationStatus.NotFound;
        }

        await _roleCache.InvalidateAsync(dto.UserPK);
        return OperationStatus.Success;
    }

    /// <inheritdoc />
    public async Task<OperationStatus> DeleteAsync(Guid userPK) {
        var existing = await _manager.GetByPKAsync(userPK);
        if (existing is null) {
            return OperationStatus.NotFound;
        }

        if (!_access.IsAllowed(ToTarget(existing), UserOperation.Delete)) {
            return OperationStatus.Forbidden;
        }

        var deleted = await _manager.DeleteAsync(userPK);
        await _roleCache.InvalidateAsync(userPK);

        return deleted ? OperationStatus.Success : OperationStatus.NotFound;
    }

    /// <summary>
    /// Converts a domain <see cref="User"/> entity to a <see cref="TargetUserInfo"/> for access checking.
    /// </summary>
    /// <param name="user">The user entity to convert.</param>
    /// <returns>A <see cref="TargetUserInfo"/> containing the user's primary key and role primary key.</returns>
    private static TargetUserInfo ToTarget(User user) => new(user.UserPK, user.RolePK);
}
