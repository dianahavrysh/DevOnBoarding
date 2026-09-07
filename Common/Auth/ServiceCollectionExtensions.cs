using System;
using Common.Auth;
using Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Common.Auth;

/// <summary>
/// Extension methods for registering JWT and password hashing services in the DI container.
/// These extensions allow the API layer to register Common layer services without knowing implementation details.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds JWT token service and password hasher to the service collection.
    /// This method must be called from Program.cs to register Common layer authentication services.
    /// </summary>
    /// <param name="services">The service collection to register services with.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown if services is null.</exception>
    public static IServiceCollection AddJwtTokenService(this IServiceCollection services)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        // Register internal JwtTokenService - clients see only IJwtTokenService interface
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        // Register password hasher implementation
        services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();

        // Validate JWT options on startup
        services.AddSingleton<IValidateOptions<JwtOptions>, JwtOptionsValidator>();

        return services;
    }
}
