using System;

namespace Common.Services.DTOs
{
    /// <summary>
    /// DTO representing a user for client consumption.
    /// Excludes sensitive fields like Password.
    /// </summary>
    public class UserDTO
    {
        public Guid UserPK { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool ActiveStatus { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string? LastName { get; set; }
        public DateTime? BirthDate { get; set; }
    }
}
