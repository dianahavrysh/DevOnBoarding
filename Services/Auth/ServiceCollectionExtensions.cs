using System;
using Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Services.Auth;

/// <summary>
/// Extension methods for registering authentication services in the DI container.
/// These extensions allow the API layer to register Services layer services without knowing implementation details.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds all authentication-related services from the Services layer to the service collection.
    /// This method must be called from Program.cs to register authentication services.
    /// </summary>
    /// <param name="services">The service collection to register services with.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown if services is null.</exception>
    public static IServiceCollection AddAuthServices(this IServiceCollection services)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        // Register internal AuthService - clients see only IAuthService interface
        services.AddScoped<IAuthService, AuthService>();

        // Register internal TokenClaimsService - clients see only ITokenClaimsService interface
        services.AddScoped<ITokenClaimsService, TokenClaimsService>();

        return services;
    }
}
