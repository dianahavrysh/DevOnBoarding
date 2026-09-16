namespace Common.DTOs;

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
