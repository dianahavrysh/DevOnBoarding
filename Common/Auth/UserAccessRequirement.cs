using System;
using Microsoft.AspNetCore.Authorization;

namespace Common.Auth;

/// <summary>
/// Specifies the operation being performed on a user resource.
/// </summary>
public enum UserOperation
{
    View,
    Edit,
    Delete
}

/// <summary>
/// Authorization requirement for evaluating user access to perform specific operations.
/// </summary>
public class UserAccessRequirement : IAuthorizationRequirement
{
    /// <summary>
    /// Gets the operation to be performed.
    /// </summary>
    public UserOperation Operation { get; }

    /// <summary>
    /// Creates a new authorization requirement for the specified operation.
    /// </summary>
    public UserAccessRequirement(UserOperation operation) => Operation = operation;
}

/// <summary>
/// Minimal data needed about the target user to make an authorization decision.
/// Avoids pulling the full User entity just to check role and primary key.
/// 
/// The RoleEnum property provides a computed, parsed version of RoleName for type-safe
/// role comparisons. If RoleName is invalid or null, RoleEnum returns null.
/// </summary>
public record TargetUserInfo(Guid UserPK, string RoleName)
{
    /// <summary>
    /// Gets the role as a strongly-typed enum value, parsed from RoleName.
    /// 
    /// This property parses RoleName to the Role enum on every access; no caching.
    /// Returns null if RoleName is null, empty, or not a canonical role string.
    /// 
    /// The UserAccessHandler does not use this property; it handles role parsing
    /// directly for better control over logging and error handling. This property
    /// exists for other code that may need the enum representation.
    /// </summary>
    public Role? RoleEnum =>
        RoleExtensions.TryParseRole(RoleName, out var role) ? role : null;
}

