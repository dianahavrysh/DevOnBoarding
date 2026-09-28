using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Common.Caching;
using Common.DTOs;
using Common.Enums;
using Common.Extensions;
using Common.Interfaces;

namespace Common.Auth.Authorization;

/// <summary>
/// Authorization handler for evaluating user access to perform specific operations.
/// </summary>
public sealed class UserAccessHandler
    : AuthorizationHandler<UserAccessRequirement, TargetUserInfo> {
    private readonly ILogger<UserAccessHandler> _logger;
    private readonly IUsersService _usersService;

    /// <summary>
    /// Creates a new instance of the <see cref="UserAccessHandler"/> class.
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="usersService"></param>
    public UserAccessHandler(
        ILogger<UserAccessHandler> logger,
        IUsersService usersService) {
        _logger = logger;
        _usersService = usersService;
    }

    /// <summary>
    /// Handles the authorization requirement for user access.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="requirement"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        UserAccessRequirement requirement,
        TargetUserInfo target) {
        var requesterId = Guid.TryParse(
            context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
            out var id)
                ? id
                : Guid.Empty;

        CachedUserRole? requesterCachedRole = null;
        try {
            requesterCachedRole = await _usersService.GetRoleAsync(requesterId);
        }
        catch (Exception ex) {
            _logger.LogError(
                ex,
                "Error retrieving cached role for requester {RequesterId}. Attempting JWT fallback.",
                requesterId);
        }

        Role? requesterRole = null;

        if (requesterCachedRole != null) {
            if (!RoleExtensions.TryParseRole(requesterCachedRole.RoleName, out var parsedRole)) {
                _logger.LogWarning(
                    "Cached role name '{RoleName}' for user {UserId} is invalid. Access denied.",
                    requesterCachedRole.RoleName,
                    requesterId);
                return;
            }
            requesterRole = parsedRole;
            _logger.LogDebug(
                "Using cached role '{RoleName}' for user {UserId}",
                requesterCachedRole.RoleName,
                requesterId);
        }
        else {
            var requesterRoleString = context.User.FindFirst(ClaimTypes.Role)?.Value;
            if (!RoleExtensions.TryParseRole(requesterRoleString, out var parsedRole)) {
                _logger.LogWarning(
                    "Invalid role '{InvalidRole}' in JWT claim for user {UserId}. Access denied.",
                    requesterRoleString,
                    requesterId);
                return;
            }
            requesterRole = parsedRole;
            _logger.LogDebug(
                "No cached role found for user {UserId}; using JWT role '{RoleName}'",
                requesterId,
                requesterRoleString);
        }

        if (!Enum.IsDefined(typeof(Role), target.RolePK)) {
            _logger.LogWarning(
                "Target user {TargetUserId} has invalid role id '{RolePK}' in database. Access denied.",
                target.UserPK,
                target.RolePK);

            return;
        }

        var targetRole = (Role)target.RolePK;

        var canManageTargetRole =
            requesterRole == Role.Administrator
            || (requesterRole == Role.Manager
                && targetRole == Role.User);

        var isSelf =
            requesterId == target.UserPK;

        bool allowed = requirement.Operation switch {
            UserOperation.View =>
                RoleHierarchy.IsAtLeast(requesterRole.Value, targetRole),

            UserOperation.Create =>
                canManageTargetRole,

            UserOperation.Edit =>
                canManageTargetRole || isSelf,

            UserOperation.Delete =>
                canManageTargetRole || isSelf,

            _ => false
        };

        if (allowed) {
            _logger.LogInformation(
                "Authorization granted: {Operation} on user {TargetUserId} by {RequesterRole}",
                requirement.Operation,
                target.UserPK,
                requesterRole);

            context.Succeed(requirement);
        }
        else {
            _logger.LogInformation(
                "Authorization denied: {Operation} on user {TargetUserId} by {RequesterRole}",
                requirement.Operation,
                target.UserPK,
                requesterRole);
        }
    }
}
