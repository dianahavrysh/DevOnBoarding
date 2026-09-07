using System;

namespace Common.Interfaces;

/// <summary>
/// Service interface for password hashing operations.
/// Responsible for securely hashing and verifying passwords.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Hashes a plain-text password using a secure algorithm.
    /// </summary>
    /// <param name="password">The plain-text password to hash.</param>
    /// <returns>The hashed password.</returns>
    string Hash(string password);

    /// <summary>
    /// Verifies a plain-text password against a stored hash.
    /// </summary>
    /// <param name="password">The plain-text password to verify.</param>
    /// <param name="hash">The stored hashed password.</param>
    /// <returns>True if the password matches the hash; false otherwise.</returns>
    bool Verify(string password, string hash);
}
