using System.Threading.Tasks;
using Common.DTOs;
using Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace API.Controllers;

/// <summary>
/// API controller for authentication operations.
/// Responsible only for HTTP routing and response formatting.
/// Delegates all authentication logic to IAuthService.
/// </summary>
[ApiController]
[AllowAnonymous]
[Route("api/[controller]")]
public sealed class AuthController : ControllerBase {
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService) {
        _authService = authService;
    }

    /// <summary>
    /// Authenticates a user and returns a JWT token.
    /// </summary>
    /// <param name="loginDto">Login credentials (email and password).</param>
    [HttpPost("login")]
    [EnableRateLimiting("login")]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginDto loginDto) {
        var token = await _authService.AuthenticateAsync(loginDto.Email, loginDto.Password);

        if (token is null) {
            return Problem(
                title: "Invalid email or password.",
                statusCode: StatusCodes.Status401Unauthorized);
        }

        return Ok(new LoginResponseDto { Token = token });
    }
}
