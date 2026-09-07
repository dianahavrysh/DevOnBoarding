using System;
using System.Security.Claims;

namespace Common.Interfaces;

/// <summary>
/// Service interface for JWT token operations.
/// Responsible for creating and validating JWT tokens.
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Generates a new JWT token for the specified user.
    /// </summary>
    /// <param name="userId">The user's primary key.</param>
    /// <param name="username">The user's username.</param>
    /// <param name="roleName">The user's role name.</param>
    /// <returns>A JWT token string.</returns>
    string GenerateToken(Guid userId, string username, string roleName);

    /// <summary>
    /// Validates a JWT token and returns the claims principal if valid.
    /// </summary>
    /// <param name="token">The JWT token string.</param>
    /// <returns>A ClaimsPrincipal if the token is valid; null otherwise.</returns>
    ClaimsPrincipal? ValidateToken(string token);
}
