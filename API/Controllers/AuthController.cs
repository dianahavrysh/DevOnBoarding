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
    /// <param name="loginDto">Login credentials (username and password).</param>
    /// <returns>A JWT token if authentication succeeds; otherwise 401 Unauthorized.</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        if (loginDto == null)
        {
            return BadRequest(new { error = "Login credentials are required." });
        }

        if (string.IsNullOrWhiteSpace(loginDto.Username) || string.IsNullOrWhiteSpace(loginDto.Password))
        {
            return BadRequest(new { error = "Username and password are required." });
        }

        var token = await _authService.AuthenticateAsync(loginDto.Username, loginDto.Password);

        if (token == null)
        {
            return Unauthorized(new { error = "Invalid username or password." });
        }

        return Ok(new LoginResponseDto { Token = token });
    }
}

/// <summary>
/// DTO for login response.
/// </summary>
public class LoginResponseDto
{
    /// <summary>
    /// The JWT token to use for authenticated requests.
    /// </summary>
    public string Token { get; set; } = string.Empty;
}

