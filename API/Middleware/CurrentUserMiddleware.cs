using System;
using System.Threading.Tasks;
using Common;
using Common.Contexts;
using Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace API.Middleware;

/// <summary>
/// Middleware that caches the current user's information for the duration of the request.
/// </summary>
public sealed class CurrentUserMiddleware {
    private readonly RequestDelegate _next;

    public CurrentUserMiddleware(RequestDelegate next) {
        _next = next;
    }

    public async Task InvokeAsync(
    HttpContext context,
    ITokenClaimsService claims,
    IUserRoleProvider roles,
    CurrentUserContext currentUser) {
        if (context.User.Identity?.IsAuthenticated != true) {
            await _next(context);
            return;
        }

        var userPK = claims.GetUserIdFromPrincipal(context.User);
        var role = userPK == Guid.Empty ? null : await roles.GetRoleAsync(userPK);

        if (role is null) {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        currentUser.Set(userPK, role.Value);
        await _next(context);
    }
}
