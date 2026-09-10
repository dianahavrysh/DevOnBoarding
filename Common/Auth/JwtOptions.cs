namespace Common.Auth;

/// <summary>
/// Configuration options for JWT token generation and validation.
/// </summary>
public class JwtOptions
{
    /// <summary>
    /// The secret key used to sign JWT tokens.
    /// Must be at least 32 characters for HS256 algorithm.
    /// </summary>
    public string Secret { get; set; } = string.Empty;

    /// <summary>
    /// The issuer of the JWT token.
    /// </summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    /// The intended audience for the JWT token.
    /// </summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// The token expiration time in minutes.
    /// </summary>
    public int ExpirationMinutes { get; set; } = 60;
}
