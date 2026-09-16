using Microsoft.Extensions.Options;

namespace Common.Auth;

/// <summary>
/// Validates JWT configuration options at startup.
/// Ensures that all required settings are present and valid.
/// </summary>
public class JwtOptionsValidator : IValidateOptions<JwtOptions>
{
    /// <summary>
    /// Validates the JWT configuration.
    /// </summary>
    /// <param name="name">The name of the options being validated.</param>
    /// <param name="options">The JWT options to validate.</param>
    /// <returns>A validation result.</returns>
    public ValidateOptionsResult Validate(string? name, JwtOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.Secret))
        {
            return ValidateOptionsResult.Fail("JWT Secret is required.");
        }

        if (options.Secret.Length < 32)
        {
            return ValidateOptionsResult.Fail("JWT Secret must be at least 32 characters long for HS256.");
        }

        if (string.IsNullOrWhiteSpace(options.Issuer))
        {
            return ValidateOptionsResult.Fail("JWT Issuer is required.");
        }

        if (string.IsNullOrWhiteSpace(options.Audience))
        {
            return ValidateOptionsResult.Fail("JWT Audience is required.");
        }

        if (options.ExpirationMinutes <= 0)
        {
            return ValidateOptionsResult.Fail("JWT ExpirationMinutes must be greater than 0.");
        }

        return ValidateOptionsResult.Success;
    }
}
