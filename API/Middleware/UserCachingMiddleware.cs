using System;
using System.Threading.Tasks;
using Common;
using Common.Contexts;
using Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace API.Middleware;

/// <summary>
/// Resolves the authenticated user's role (cache first) and publishes it via ServerContext.
/// Anonymous requests pass through untouched. Unknown or inactive users get 401.
/// Must be placed after UseAuthentication and before UseAuthorization.
/// </summary>
public sealed class UserCachingMiddleware {
    private readonly RequestDelegate _next;

    public UserCachingMiddleware(RequestDelegate next) {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context,
        ITokenClaimsService claims,
        IUserRoleProvider roles) {
        if (context.User.Identity?.IsAuthenticated != true) {
            await _next(context);
            return;
        }

        var userPK = claims.GetUserIdFromPrincipal(context.User);
        var rolePK = userPK == Guid.Empty
            ? null
            : await roles.GetAsync(userPK);

        if (rolePK is null) {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        using (ServerContext.Begin(userPK, rolePK.Value)) {
            await _next(context);
        }
    }
}
