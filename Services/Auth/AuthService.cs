using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.DTOs;
using Common.Entities;
using Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace Services.Auth;

/// <summary>
/// Application service for user authentication.
/// Responsible for authenticating users and generating JWT tokens.
/// Depends only on Common abstractions and database managers.
/// </summary>
internal class AuthService : IAuthService
{
    private readonly IUsersManager _usersManager;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUsersManager usersManager,
        IJwtTokenService jwtTokenService,
        ILogger<AuthService> logger)
    {
        _usersManager = usersManager;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
    }

    /// <summary>
    /// Authenticates a user by username and password, and returns a JWT token if successful.
    /// </summary>
    public async Task<string?> AuthenticateAsync(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            _logger.LogWarning("Authentication attempt with empty username or password.");
            return null;
        }

        try
        {
            // Retrieve user from database by username
            // Note: In a real application, there should be a GetByUsernameAsync method
            // For now, we retrieve all users and filter (in production, use GetByUsernameAsync)
            var user = await GetUserByUsernameAsync(username);

            if (user == null)
            {
                _logger.LogWarning("Authentication failed: User not found for username '{Username}'.", username);
                return null;
            }

            // Verify password (assuming password in database is hashed)
            // In production, use BCrypt.Net-Next or similar for verification
            if (!VerifyPassword(password, user.Password))
            {
                _logger.LogWarning("Authentication failed: Invalid password for username '{Username}'.", username);
                return null;
            }

            // Check if user is active
            if (!user.ActiveStatus)
            {
                _logger.LogWarning("Authentication failed: User '{Username}' is inactive.", username);
                return null;
            }

            // Generate JWT token with user information
            var token = _jwtTokenService.GenerateToken(user.UserPK, user.UserName, user.RoleName);

            _logger.LogInformation("User '{Username}' authenticated successfully.", username);
            return token;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during authentication for username '{Username}'.", username);
            return null;
        }
    }

    /// <summary>
    /// Retrieves a user from the database by username.
    /// This is a temporary implementation until a GetByUsernameAsync method is added to IUsersManager.
    /// </summary>
    private async Task<User?> GetUserByUsernameAsync(string username)
    {
        // Get all users (pagination) - this is a workaround
        // In production, add a GetByUsernameAsync(string username) method to IUsersManager
        var requestingUserPK = Guid.Empty; // System user for internal queries
        var (users, _) = await _usersManager.GetByPageAsync(
            requestingUserPK,
            currentPage: 1,
            pageSize: 1000,
            sortExpression: null,
            searchValue: username,
            searchByFields: new Dictionary<string, bool> { { "UserName", true } },
            includeInactive: true,
            strictMatch: true);

        foreach (var user in users)
        {
            if (user.UserName.Equals(username, StringComparison.OrdinalIgnoreCase))
            {
                return user;
            }
        }

        return null;
    }

    /// <summary>
    /// Verifies the provided password against the stored hashed password.
    /// In production, use BCrypt.Net-Next or similar for proper password verification.
    /// </summary>
    private bool VerifyPassword(string providedPassword, string storedHashedPassword)
    {
        // Placeholder: In production, use BCrypt.VerifyHashedPassword or similar
        // For now, do a simple comparison (NOT SECURE - for development only)
        // Example: return BCrypt.Net.BCrypt.Verify(providedPassword, storedHashedPassword);

        return storedHashedPassword == providedPassword;
    }
}
