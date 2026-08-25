using System;

namespace Common.Services.DTOs
{
    /// <summary>
    /// DTO used for creating or updating a user.
    /// Contains write-only fields (Password, RoleTypePK) not exposed on read.
    /// </summary>
    public class UserCreateUpdateDTO
    {
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool ActiveStatus { get; set; }
        public Guid RoleTypePK { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string? LastName { get; set; }
        public DateTime? BirthDate { get; set; }
    }
}
