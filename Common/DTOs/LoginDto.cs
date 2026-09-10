using System.ComponentModel.DataAnnotations;

namespace Common.DTOs;

/// <summary>
/// DTO for login request.
/// </summary>
public class LoginDto
{
    /// <summary>
    /// The user's email address.
    /// </summary>
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Email must be a valid email address.")]
    [StringLength(254, MinimumLength = 5, ErrorMessage = "Email must be between 5 and 254 characters.")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// The user's plain-text password.
    /// </summary>
    [Required(ErrorMessage = "Password is required.")]
    [StringLength(256, MinimumLength = 1, ErrorMessage = "Password must be between 1 and 256 characters.")]
    public string Password { get; set; } = string.Empty;
}
