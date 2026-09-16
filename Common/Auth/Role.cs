namespace Common.Auth;

/// <summary>
/// Strongly-typed role enumeration with hierarchical privilege levels.
/// 
/// Enum values represent privilege levels within the application:
/// - Higher numeric values = higher privilege
/// - These enum values are an INTERNAL implementation detail
/// 
/// Canonical string representations ("Administrator", "Manager", "User") are used
/// at database and API boundaries. Conversion between enum and string is handled
/// exclusively by RoleExtensions.TryParseRole() and ToDisplayString().
/// 
/// Hierarchy:
///   Administrator (3) — Highest privilege
///   Manager (2) — Middle privilege
///   User (1) — Lowest privilege
/// 
/// Direct enum comparison for privilege checks:
///   Example: if (userRole >= Role.Manager) { /* grant access */ }
/// 
/// For role name conversions, see RoleExtensions.TryParseRole() and ToDisplayString().
/// </summary>
public enum Role : byte
{
    /// <summary>
    /// User role — lowest privilege level.
    /// Can only view and edit their own profile.
    /// </summary>
    User = 1,

    /// <summary>
    /// Manager role — middle privilege level.
    /// Can view and edit other users, but cannot delete them.
    /// Can only delete User-level users, not other managers or administrators.
    /// </summary>
    Manager = 2,

    /// <summary>
    /// Administrator role — highest privilege level.
    /// Can view, edit, and delete any user.
    /// Full system access for user management.
    /// </summary>
    Administrator = 3
}
