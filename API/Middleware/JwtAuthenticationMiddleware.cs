using System;
using System.Threading.Tasks;
using Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace API.Middleware;

/// <summary>
/// Middleware for JWT token authentication in the HTTP request pipeline.
/// Responsible only for HTTP pipeline concerns:
/// - Extracting bearer token from Authorization header
/// - Validating token using IJwtTokenService
/// - Setting HttpContext.User if valid
/// Does NOT contain business logic or token validation implementation details.
/// </summary>
public class JwtAuthenticationMiddleware
{
    private readonly RequestDelegate _next;

    public JwtAuthenticationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    /// <summary>
    /// Processes the HTTP request to extract and validate JWT token.
    /// </summary>
    public async Task InvokeAsync(HttpContext context, IJwtTokenService jwtTokenService)
    {
        string? authHeader = context.Request.Headers.Authorization;

        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            var token = authHeader["Bearer ".Length..];
            var principal = jwtTokenService.ValidateToken(token);

            if (principal != null)
            {
                context.User = principal;
            }
        }

        await _next(context);
    }
}

