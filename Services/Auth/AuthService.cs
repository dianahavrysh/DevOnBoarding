using System;
using System.Collections.Generic;
using System.Security.Claims;
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
    /// Authenticates a user by email and password, and returns a JWT token if successful.
    /// </summary>
    public async Task<string?> AuthenticateAsync(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            _logger.LogWarning("Authentication attempt with empty username or password.");
            return null;
        }

        // Retrieve user from database by email
        var user = await _usersManager.GetByEmailAsync(username);

        if (user == null)
        {
            _logger.LogWarning("Authentication failed: User not found for username '{Username}'.", username);
            return null;
        }

        // Verify password (plaintext comparison)
        if (user.Password != password)
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

        // Create application-specific claims
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserPK.ToString()),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Role, user.RoleName)
        };

        // Generate JWT token with claims
        var token = _jwtTokenService.GenerateToken(claims);

        _logger.LogInformation("User '{Username}' authenticated successfully.", username);
        return token;
    }
}
