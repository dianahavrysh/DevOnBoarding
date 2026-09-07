using System.Collections.Generic;
using System.Security.Claims;

namespace Common.Interfaces;

/// <summary>
/// Service interface for JWT token operations.
/// Responsible for creating and validating JWT tokens.
/// Does NOT decide which application-specific claims should exist.
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Generates a new JWT token with the provided claims.
    /// </summary>
    /// <param name="claims">The claims to include in the token.</param>
    /// <returns>A JWT token string.</returns>
    string GenerateToken(IEnumerable<Claim> claims);

    /// <summary>
    /// Validates a JWT token and returns the claims principal if valid.
    /// </summary>
    /// <param name="token">The JWT token string.</param>
    /// <returns>A ClaimsPrincipal if the token is valid; null otherwise.</returns>
    ClaimsPrincipal? ValidateToken(string token);
}
