using System;

namespace Common.Entities
{
    /// <summary>
    /// Domain entity representing a user record stored in the database.
    /// This type is used internally by business logic and data access layers.
    /// </summary>
    public class User
    {
        /// <summary>Primary key for the user.</summary>
        public Guid UserPK { get; set; }
        /// <summary>Login user name.</summary>
        public string UserName { get; set; } = string.Empty;
        /// <summary>User email address.</summary>
        public string Email { get; set; } = string.Empty;
        /// <summary>Hashed password (not exposed to clients).</summary>
        public string Password { get; set; } = string.Empty;
        /// <summary>Whether the user account is active.</summary>
        public bool ActiveStatus { get; set; }
        /// <summary>Foreign key to role type metadata.</summary>
        public Guid RoleTypePK { get; set; }
        /// <summary>Human-readable role name.</summary>
        public string RoleName { get; set; } = string.Empty;
        /// <summary>User's first name.</summary>
        public string FirstName { get; set; } = string.Empty;
        /// <summary>User's last/second name (optional).</summary>
        public string? SecondName { get; set; }
        /// <summary>User's birth date (optional).</summary>
        public DateTime? BirthDate { get; set; }
    }
}
