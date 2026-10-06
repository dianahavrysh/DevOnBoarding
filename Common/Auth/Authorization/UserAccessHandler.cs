using System.Threading.Tasks;
using Common.DTOs;
using Common.Enums;
using Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace Common.Auth.Authorization;

/// <summary>
/// Evaluates whether the current user (from <see cref="ICurrentUserContext"/>)
/// may perform the requested operation on the target user.
/// </summary>
public sealed class UserAccessHandler
    : AuthorizationHandler<UserAccessRequirement, TargetUserInfo> {
    private readonly ICurrentUserContext _currentUser;
    private readonly ILogger<UserAccessHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserAccessHandler"/> class.
    /// </summary>
    /// <param name="currentUser">The user who issued the current request.</param>
    /// <param name="logger">Logger.</param>
    public UserAccessHandler(
        ICurrentUserContext currentUser,
        ILogger<UserAccessHandler> logger) {
        _currentUser = currentUser;
        _logger = logger;
    }

    /// <summary>
    /// Determines whether the current user may perform the operation on the target user.
    /// </summary>
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        UserAccessRequirement requirement,
        TargetUserInfo target) {

        if (!_currentUser.IsInitialized) {
            _logger.LogWarning("Authorization denied: current user context is not initialized.");
            return Task.CompletedTask;
        }

        if (target.RoleEnum is not { } targetRole) {
            _logger.LogWarning(
                "Authorization denied: target {TargetUserId} has invalid role id {RolePK}.",
                target.UserPK, target.RolePK);
            return Task.CompletedTask;
        }

        var requesterRole = _currentUser.Role;

        var canManageTargetRole =
            requesterRole == Role.Administrator
            || (requesterRole == Role.Manager && targetRole == Role.User);

        var isSelf = _currentUser.UserPK == target.UserPK;

        var allowed = requirement.Operation switch {
            UserOperation.View => RoleHierarchy.IsAtLeast(requesterRole, targetRole),
            UserOperation.Create or UserOperation.AssignRole => canManageTargetRole,
            UserOperation.Edit or UserOperation.Delete => canManageTargetRole || isSelf,
            _ => false
        };

        if (allowed) {
            _logger.LogDebug(
                "Authorization granted: {Operation} on user {TargetUserId} by {RequesterRole}",
                requirement.Operation, target.UserPK, requesterRole);
            context.Succeed(requirement);
        }
        else {
            _logger.LogInformation(
                "Authorization denied: {Operation} on user {TargetUserId} by {RequesterRole}",
                requirement.Operation, target.UserPK, requesterRole);
        }

        return Task.CompletedTask;
    }
}
