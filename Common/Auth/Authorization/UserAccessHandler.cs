using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Common.DTOs;
using Common.Enums;
using Common.Extensions;

namespace Common.Auth.Authorization;

/// <summary>
/// Authorization handler for evaluating user access to perform specific operations.
/// </summary>
public sealed class UserAccessHandler
    : AuthorizationHandler<UserAccessRequirement, TargetUserInfo> {
    private readonly ILogger<UserAccessHandler> _logger;

    /// <summary>
    /// Creates a new instance of the <see cref="UserAccessHandler"/> class.
    /// </summary>
    /// <param name="logger"></param>
    public UserAccessHandler(ILogger<UserAccessHandler> logger) {
        _logger = logger;
    }

    /// <summary>
    /// Handles the authorization requirement for user access.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="requirement"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        UserAccessRequirement requirement,
        TargetUserInfo target) {
        var requesterRoleString =
            context.User.FindFirst(ClaimTypes.Role)?.Value;

        var requesterId = Guid.TryParse(
            context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
            out var id)
                ? id
                : Guid.Empty;

        if (!RoleExtensions.TryParseRole(
                requesterRoleString,
                out var requesterRole)) {
            _logger.LogWarning(
                "Invalid role '{InvalidRole}' in JWT claim for user {UserId}. Access denied.",
                requesterRoleString,
                requesterId);

            return Task.CompletedTask;
        }

        if (!Enum.IsDefined(typeof(Role), target.RoleId)) {
            _logger.LogWarning(
                "Target user {TargetUserId} has invalid role id '{RoleId}' in database. Access denied.",
                target.UserPK,
                target.RoleId);

            return Task.CompletedTask;
        }

        var targetRole = (Role)target.RoleId;

        var canManageTargetRole =
            requesterRole == Role.Administrator
            || (requesterRole == Role.Manager
                && targetRole == Role.User);

        var isSelf =
            requesterId == target.UserPK;

        bool allowed = requirement.Operation switch {
            UserOperation.View =>
                RoleHierarchy.IsAtLeast(requesterRole, targetRole),

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

        return Task.CompletedTask;
    }
}
