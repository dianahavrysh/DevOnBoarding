using System;
using System.Threading.Tasks;
using Common.Contexts;
using Common.DTOs;
using Common.Enums;
using Common.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace Common.Auth.Authorization;

/// <summary>
/// Evaluates whether the current user (from <see cref="ServerContext"/>)
/// may perform the requested operation on the target user.
/// </summary>
public sealed class UserAccessHandler
    : AuthorizationHandler<UserAccessRequirement, TargetUserInfo> {
    private readonly ILogger<UserAccessHandler> _logger;

    public UserAccessHandler(ILogger<UserAccessHandler> logger) {
        _logger = logger;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        UserAccessRequirement requirement,
        TargetUserInfo target) {
        if (!ServerContext.IsAuthenticated) {
            _logger.LogWarning("Authorization denied: ServerContext is not initialized.");
            return Task.CompletedTask;
        }

        if (!Enum.IsDefined(typeof(Role), ServerContext.RolePK)) {
            _logger.LogWarning(
                "Authorization denied: requester {RequesterId} has invalid role id {RolePK}.",
                ServerContext.UserPK,
                ServerContext.RolePK);
            return Task.CompletedTask;
        }

        if (target.RoleEnum is not { } targetRole) {
            _logger.LogWarning(
                "Authorization denied: target {TargetUserId} has invalid role id {RolePK}.",
                target.UserPK,
                target.RolePK);
            return Task.CompletedTask;
        }

        var requesterRole = (Role)ServerContext.RolePK;

        var canManageTargetRole =
            requesterRole == Role.Administrator
            || (requesterRole == Role.Manager && targetRole == Role.User);

        var isSelf = ServerContext.UserPK == target.UserPK;

        var allowed = requirement.Operation switch {
            UserOperation.View => RoleHierarchy.IsAtLeast(requesterRole, targetRole),
            UserOperation.Create => canManageTargetRole,
            UserOperation.Edit => canManageTargetRole || isSelf,
            UserOperation.Delete => canManageTargetRole || isSelf,
            _ => false
        };

        _logger.LogInformation(
            "Authorization {Result}: {Operation} on user {TargetUserId} by {RequesterRole}",
            allowed ? "granted" : "denied",
            requirement.Operation,
            target.UserPK,
            requesterRole);

        if (allowed) {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
