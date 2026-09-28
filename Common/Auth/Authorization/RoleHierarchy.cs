using Common.Enums;

namespace Common.Auth.Authorization;

/// <summary>
/// Defines the hierarchical ordering of roles (higher privilege = higher enum value).
/// 
/// This class replaces the previous string-based Dictionary with direct enum comparisons
/// for type safety and performance.
/// 
/// Hierarchy Reference:
/// - Administrator = 3 (highest privilege)
/// - Manager = 2 (middle privilege)
/// - User = 1 (lowest privilege)
/// 
/// Usage: Use IsAtLeast() to check if a requester has permission to perform
/// an operation on a target resource based on role hierarchy.
/// </summary>
public static class RoleHierarchy
{
    /// <summary>
    /// Gets the hierarchical privilege level of a role.
    /// Higher numeric values indicate higher privilege.
    /// 
    /// This method exists for clarity and consistency but is typically not needed
    /// in application code; prefer direct enum comparison.
    /// </summary>
    /// <param name="role">The Role enum value</param>
    /// <returns>The privilege level (User=1, Manager=2, Administrator=3)</returns>
    public static byte LevelOf(Role role) => (byte)role;

    /// <summary>
    /// Checks if a requester's role has at least the privilege level required.
    /// 
    /// This is the APPROVED method for hierarchical role comparisons.
    /// 
    /// Example:
    /// - RoleHierarchy.IsAtLeast(Role.Manager, Role.User) returns true
    /// - RoleHierarchy.IsAtLeast(Role.User, Role.Manager) returns false
    /// - RoleHierarchy.IsAtLeast(Role.Administrator, Role.Administrator) returns true
    /// </summary>
    /// <param name="requesterRole">The role to check</param>
    /// <param name="minimumRole">The minimum required role privilege level</param>
    /// <returns>true if requesterRole privilege >= minimumRole privilege</returns>
    public static bool IsAtLeast(Role requesterRole, Role minimumRole) =>
        (byte)requesterRole >= (byte)minimumRole;
}
