using System;
using System.Security.Claims;
using Common.Interfaces;

namespace Services.Auth;

/// <summary>
/// Service for extracting user information from authenticated JWT claims.
/// Responsible for retrieving user details from ClaimsPrincipal.
/// </summary>
internal class TokenClaimsService : ITokenClaimsService
{
    /// <summary>
    /// Extracts the user's primary key from the claims principal.
    /// </summary>
    public Guid GetUserIdFromPrincipal(ClaimsPrincipal principal)
    {
        if (principal == null)
        {
            throw new ArgumentNullException(nameof(principal));
        }

        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            throw new InvalidOperationException("User ID claim not found or invalid.");
        }

        return userId;
    }

    /// <summary>
    /// Extracts the user's username from the claims principal.
    /// </summary>
    public string? GetUsernameFromPrincipal(ClaimsPrincipal principal)
    {
        if (principal == null)
        {
            throw new ArgumentNullException(nameof(principal));
        }

        var usernameClaim = principal.FindFirst(ClaimTypes.Name);
        return usernameClaim?.Value;
    }
}
