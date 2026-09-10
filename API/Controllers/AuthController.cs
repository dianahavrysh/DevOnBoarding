using System.Threading.Tasks;
using Common.DTOs;
using Common.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

/// <summary>
/// API controller for authentication operations.
/// Responsible only for HTTP routing and response formatting.
/// Delegates all authentication logic to IAuthService.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Authenticates a user and returns a JWT token.
    /// </summary>
    /// <param name="loginDto">Login credentials (email and password).</param>
    /// <returns>A JWT token if authentication succeeds; otherwise 401 Unauthorized.</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var token = await _authService.AuthenticateAsync(loginDto.Email, loginDto.Password);

        return token is not null
            ? Ok(new LoginResponseDto { Token = token })
            : Unauthorized(new { error = "Invalid email or password." });
    }
}
