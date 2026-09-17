using System;

namespace Common.DTOs
{
    /// <summary>
    /// DTO representing a user for client consumption.
    /// Excludes sensitive fields such as the password. Used for read operations.
    /// </summary>
    public class UserDTO
    {
        /// <summary>User primary key.</summary>
        public Guid UserPK { get; set; }
        /// <summary>Login user name.</summary>
        public string UserName { get; set; } = string.Empty;
        /// <summary>User email address.</summary>
        public string Email { get; set; } = string.Empty;
        /// <summary>Whether the user is active.</summary>
        public bool ActiveStatus { get; set; }
        /// <summary>
        /// User's role name (e.g., "Administrator", "Manager", "User").
        /// See RoleExtensions.TryParseRole() to safely convert this string to a Role enum value.
        /// </summary>
        public string RoleName { get; set; } = string.Empty;
        /// <summary>
        /// Role id (dbo.Roles.Id: 1=User, 2=Manager, 3=Administrator), exposed so clients
        /// can pre-select the correct role when editing a user.
        /// </summary>
        public byte RoleId { get; set; }
        /// <summary>User first name.</summary>
        public string FirstName { get; set; } = string.Empty;
        /// <summary>User last name (optional).</summary>
        public string? SecondName { get; set; }
        /// <summary>User birth date (optional).</summary>
        public DateTime? BirthDate { get; set; }
    }
}
