using System;
using System.Security.Claims;

namespace Common.Interfaces;

/// <summary>
/// Service interface for extracting application-relevant claims from an authenticated principal.
/// Responsible for retrieving user information from JWT claims.
/// </summary>
public interface ITokenClaimsService
{
    /// <summary>
    /// Extracts the user's primary key from the authenticated claims principal.
    /// </summary>
    /// <param name="principal">The claims principal from an authenticated JWT token.</param>
    /// <returns>The user's primary key (Guid).</returns>
    Guid GetUserIdFromPrincipal(ClaimsPrincipal principal);

    /// <summary>
    /// Extracts the user's username from the authenticated claims principal.
    /// </summary>
    /// <param name="principal">The claims principal from an authenticated JWT token.</param>
    /// <returns>The user's username.</returns>
    string? GetUsernameFromPrincipal(ClaimsPrincipal principal);
}
