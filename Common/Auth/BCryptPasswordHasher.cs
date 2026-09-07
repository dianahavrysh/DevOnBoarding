using System;
using Common.Interfaces;
using BCrypt.Net;

namespace Common.Auth;

/// <summary>
/// Password hasher implementation using BCrypt.Net-Next.
/// Provides secure password hashing and verification using BCrypt algorithm.
/// </summary>
internal class BCryptPasswordHasher : IPasswordHasher
{
    /// <summary>
    /// Hashes a plain-text password using BCrypt with a work factor of 12.
    /// </summary>
    /// <param name="password">The plain-text password to hash.</param>
    /// <returns>The BCrypt-hashed password.</returns>
    /// <exception cref="ArgumentException">Thrown if password is null or empty.</exception>
    public string Hash(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("Password cannot be null or empty.", nameof(password));
        }

        return BCrypt.EnhancedHashPassword(password, workFactor: 12);
    }

    /// <summary>
    /// Verifies a plain-text password against a BCrypt hash.
    /// </summary>
    /// <param name="password">The plain-text password to verify.</param>
    /// <param name="hash">The BCrypt hash to verify against.</param>
    /// <returns>True if the password matches the hash; false otherwise.</returns>
    public bool Verify(string password, string hash)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hash))
        {
            return false;
        }

        try
        {
            return BCrypt.EnhancedVerifyHashedPassword(hash, password) != PasswordVerificationResult.Failed;
        }
        catch
        {
            return false;
        }
    }
}
