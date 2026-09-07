namespace Common.DTOs;

/// <summary>
/// DTO for login request.
/// </summary>
public class LoginDto
{
    /// <summary>
    /// The user's username.
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// The user's plain-text password.
    /// </summary>
    public string Password { get; set; } = string.Empty;
}
