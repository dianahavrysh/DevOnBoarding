using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Common.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace Common.Auth;

/// <summary>
/// Authorization handler for evaluating user access to perform specific operations.
/// Implements role-based and identity-based authorization policies.
/// 
/// This handler converts string role claims to strongly-typed Role enum values
/// and performs all privilege checks using enum-based comparisons for type safety.
/// </summary>
public sealed class UserAccessHandler : AuthorizationHandler<UserAccessRequirement, TargetUserInfo>
{
    private readonly ILogger<UserAccessHandler> _logger;

    public UserAccessHandler(ILogger<UserAccessHandler> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Evaluates whether the current user is authorized to perform the required operation on the target user.
    /// 
    /// Authorization decisions are made by converting string role claims to strongly-typed Role enum values
    /// and comparing them using the RoleHierarchy utility.
    /// </summary>
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        UserAccessRequirement requirement,
        TargetUserInfo target)
    {
        var requesterRoleString = context.User.FindFirst(ClaimTypes.Role)?.Value;
        var requesterId = Guid.TryParse(
            context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
            out var id) ? id : Guid.Empty;

        // Parse requester role claim; deny access if invalid
        if (!RoleExtensions.TryParseRole(requesterRoleString, out var requesterRole))
        {
            _logger.LogWarning(
                "Invalid role '{InvalidRole}' in JWT claim for user {UserId}. Access denied.",
                requesterRoleString,
                requesterId);
            return Task.CompletedTask;
        }

        // Parse target user role; deny access if target role is invalid
        if (!RoleExtensions.TryParseRole(target.RoleName, out var targetRole))
        {
            _logger.LogWarning(
                "Target user {TargetUserId} has invalid role '{InvalidRole}' in database. Access denied.",
                target.UserPK,
                target.RoleName);
            return Task.CompletedTask;
        }

        bool allowed = requirement.Operation switch
        {
            // View: Requester must have at least the same role level as target
            UserOperation.View => RoleHierarchy.IsAtLeast(requesterRole, targetRole),

            // Edit: Administrator can edit anyone, Manager can edit Users and themselves, User can edit themselves
            UserOperation.Edit =>
                requesterRole == Role.Administrator
                || (requesterRole == Role.Manager && targetRole == Role.User)
                || (requesterRole == Role.User && requesterId == target.UserPK),

            // Delete: Administrator can delete anyone, Manager can delete Users only
            UserOperation.Delete =>
                requesterRole == Role.Administrator
                || (requesterRole == Role.Manager && targetRole == Role.User),

            _ => false
        };

        if (allowed)
        {
            _logger.LogInformation(
                "Authorization granted: {Operation} on user {TargetUserId} by {RequesterRole}",
                requirement.Operation,
                target.UserPK,
                requesterRole);
            context.Succeed(requirement);
        }
        else
        {
            _logger.LogInformation(
                "Authorization denied: {Operation} on user {TargetUserId} by {RequesterRole}",
                requirement.Operation,
                target.UserPK,
                requesterRole);
        }

        return Task.CompletedTask;
    }
}
