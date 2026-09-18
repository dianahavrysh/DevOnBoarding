using System;
using Common.Enums;

namespace Common.DTOs;

/// <summary>
/// Minimal data needed about the target user to make an authorization decision.
/// </summary>
public record TargetUserInfo(Guid UserPK, byte RoleId) {
    /// <summary>
    /// Gets the target user's role as a strongly-typed enum.
    /// Returns null when the role id is not defined.
    /// </summary>
    public Role? RoleEnum =>
        Enum.IsDefined(typeof(Role), RoleId)
            ? (Role)RoleId
            : null;
}
