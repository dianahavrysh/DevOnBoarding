using Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Services;

public static class ServiceCollectionExtensions {
    public static IServiceCollection AddUserServices(this IServiceCollection services) {
        services.AddScoped<IUserRoleProvider, UserRoleProvider>();
        services.AddScoped<IUsersService, UsersService>();

        return services;
    }
}
