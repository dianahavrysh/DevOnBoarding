using System;

namespace Common.Entities
{
    /// <summary>
    /// Domain entity representing a user.
    /// </summary>
    public class User
    {
        public Guid UserPK { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool ActiveStatus { get; set; }
        public Guid RoleTypePK { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string? SecondName { get; set; }
        public DateTime? BirthDate { get; set; }
    }
}
