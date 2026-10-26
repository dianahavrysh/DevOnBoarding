using Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Services.Authorization;
using Services.Caching;

namespace Services;

public static class ServiceCollectionExtensions {
    /// <summary>
    /// Registers caching services: AddStackExchangeRedisCache, IUserRoleCacheService,
    /// and binds CacheOptions from configuration.
    /// </summary>
    public static IServiceCollection AddCaching(this IServiceCollection services, IConfiguration configuration) {
        services.AddStackExchangeRedisCache(options => {
            var redisConnectionString = configuration.GetConnectionString("Redis");
            if (!string.IsNullOrEmpty(redisConnectionString)) {
                options.Configuration = redisConnectionString;
            }
        });

        services.AddOptions<CacheOptions>()
            .Bind(configuration.GetSection("Cache"))
            .ValidateOnStart();

        services.AddScoped<IUserRoleCacheService, DistributedUserRoleCacheService>();

        return services;
    }

    public static IServiceCollection AddUserServices(this IServiceCollection services, IConfiguration configuration) {
        services.AddCaching(configuration);
        services.AddScoped<IUserRoleProvider, UserRoleProvider>();
        services.AddScoped<IUserAccessChecker, UserAccessChecker>();
        services.AddScoped<IUsersService, UsersService>();

        return services;
    }
}
