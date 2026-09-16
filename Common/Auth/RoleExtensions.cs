using System;

namespace Common.Auth;

/// <summary>
/// Extension methods for converting between Role enum values and their canonical string representations.
/// 
/// This class is the ONLY approved boundary for crossing between the internal Role enum type
/// and the external string representation used in the database, JWT claims, and API responses.
/// 
/// - TryParseRole: Non-throwing version for untrusted input (e.g., JWT claims, user input)
/// - ParseRole: Throwing version for code that expects valid input (e.g., seed data)
/// - ToDisplayString: Converts enum back to canonical string for output
/// </summary>
public static class RoleExtensions
{
    /// <summary>
    /// Attempts to parse a string into a Role enum value using the canonical role names.
    /// 
    /// This is the APPROVED method for parsing untrusted role input (e.g., from JWT claims,
    /// database hydration, or user-supplied data). It never throws; instead, returns false
    /// and sets role to Role.User if the input is invalid.
    /// 
    /// Valid input strings (case-insensitive):
    /// - "Administrator"
    /// - "Manager"
    /// - "User"
    /// 
    /// Any other input (null, empty, non-canonical) returns false.
    /// </summary>
    /// <param name="roleName">The role name string to parse (case-insensitive)</param>
    /// <param name="role">The output Role enum value if parsing succeeds; Role.User if it fails</param>
    /// <returns>true if roleName matches a canonical role string; false otherwise</returns>
    public static bool TryParseRole(string? roleName, out Role role)
    {
        if (string.IsNullOrWhiteSpace(roleName))
        {
            role = Role.User; // Default to lowest privilege on null/empty
            return false;
        }

        // Case-insensitive comparison matching RoleNames behavior
        return roleName.Equals("Administrator", StringComparison.OrdinalIgnoreCase)
            ? (role = Role.Administrator, true).Item2
            : roleName.Equals("Manager", StringComparison.OrdinalIgnoreCase)
                ? (role = Role.Manager, true).Item2
                : roleName.Equals("User", StringComparison.OrdinalIgnoreCase)
                    ? (role = Role.User, true).Item2
                    : (role = Role.User, false).Item2;
    }

    /// <summary>
    /// Parses a string into a Role enum value, throwing an exception if the input is invalid.
    /// 
    /// This is the APPROVED method for parsing input that is expected to be valid
    /// (e.g., hardcoded seed data, validated configuration). For untrusted input,
    /// use TryParseRole() instead.
    /// 
    /// Valid input strings (case-insensitive):
    /// - "Administrator"
    /// - "Manager"
    /// - "User"
    /// </summary>
    /// <param name="roleName">The role name string to parse (case-insensitive)</param>
    /// <returns>The parsed Role enum value</returns>
    /// <exception cref="InvalidOperationException">Thrown if roleName does not match a canonical role</exception>
    public static Role ParseRole(string? roleName)
    {
        if (TryParseRole(roleName, out var role) && !string.IsNullOrWhiteSpace(roleName))
        {
            // Additional check: ensure the input was actually a canonical string, not null/empty
            if (roleName.Equals("Administrator", StringComparison.OrdinalIgnoreCase)
                || roleName.Equals("Manager", StringComparison.OrdinalIgnoreCase)
                || roleName.Equals("User", StringComparison.OrdinalIgnoreCase))
            {
                return role;
            }
        }

        throw new InvalidOperationException(
            $"Invalid role name '{roleName}'. Valid roles are: Administrator, Manager, User.");
    }

    /// <summary>
    /// Converts a Role enum value to its canonical string representation.
    /// 
    /// This method produces the exact string values used in the database (RoleTypes.RoleName),
    /// JWT claims (ClaimTypes.Role), and API responses. The output is deterministic and
    /// must never change, as it forms a contract with consumers.
    /// 
    /// This is the ONLY approved way to serialize a Role enum to its external string form.
    /// </summary>
    /// <param name="role">The Role enum value to convert</param>
    /// <returns>The canonical string representation ("Administrator", "Manager", or "User")</returns>
    public static string ToDisplayString(this Role role) => role switch
    {
        Role.Administrator => "Administrator",
        Role.Manager => "Manager",
        Role.User => "User",
        _ => throw new InvalidOperationException($"Unknown role value: {role}")
    };
}
