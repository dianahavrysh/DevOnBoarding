using System;
using Common.Enums;
using Common.Extensions;

namespace Common.DTOs;

/// <summary>
/// Minimal data needed about the target user to make an authorization decision.
/// </summary>
public record TargetUserInfo(Guid UserPK, byte RolePK) {
    /// <summary>
    /// Gets the target user's role as a strongly-typed enum.
    /// Returns null when the role id is not defined.
    /// </summary>
    public Role? RoleEnum =>
        RoleExtensions.TryFromPK(RolePK, out var role) ? role : null;
}
