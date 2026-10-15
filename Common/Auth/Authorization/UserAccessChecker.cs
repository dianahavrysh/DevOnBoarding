using Common.DTOs;
using Common.Enums;
using Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace Common.Auth.Authorization;

public sealed class UserAccessChecker : IUserAccessChecker {
    private readonly ICurrentUserContext _currentUser;
    private readonly ILogger<UserAccessChecker> _logger;

    public UserAccessChecker(
        ICurrentUserContext currentUser,
        ILogger<UserAccessChecker> logger) {
        _currentUser = currentUser;
        _logger = logger;
    }

    public bool IsAllowed(TargetUserInfo target, UserOperation operation) {
        if (!_currentUser.IsInitialized) {
            _logger.LogWarning("Authorization denied: current user context is not initialized.");
            return false;
        }

        if (target.RoleEnum is not { } targetRole) {
            _logger.LogWarning(
                "Authorization denied: target {TargetUserId} has invalid role id {RolePK}.",
                target.UserPK, target.RolePK);
            return false;
        }

        var requesterRole = _currentUser.Role;

        var canManageTargetRole =
            requesterRole == Role.Administrator
            || (requesterRole == Role.Manager && targetRole == Role.User);

        var isSelf = _currentUser.UserPK == target.UserPK;

        var allowed = operation switch {
            UserOperation.View => RoleHierarchy.IsAtLeast(requesterRole, targetRole),
            UserOperation.Create or UserOperation.AssignRole => canManageTargetRole,
            UserOperation.Edit or UserOperation.Delete => canManageTargetRole || isSelf,
            _ => false
        };

        _logger.LogDebug(
            "Authorization {Result}: {Operation} on user {TargetUserId} by {RequesterRole}",
            allowed ? "granted" : "denied", operation, target.UserPK, requesterRole);

        return allowed;
    }
}
