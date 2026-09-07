using System;
using System.Threading.Tasks;

namespace Common.Interfaces;

/// <summary>
/// Service interface for user authentication operations.
/// Responsible for authenticating users and generating JWT tokens.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Authenticates a user based on username and password.
    /// Returns a JWT token if authentication succeeds.
    /// </summary>
    /// <param name="username">The user's username.</param>
    /// <param name="password">The user's plain-text password.</param>
    /// <returns>A JWT token string if authentication succeeds; null otherwise.</returns>
    Task<string?> AuthenticateAsync(string username, string password);
}
