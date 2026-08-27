using System;

namespace Common.DTOs {
    /// <summary>
    /// DTO used for creating or updating a user.
    /// </summary>
    public class UserCreateUpdateDTO {
        /// <summary>
        /// Primary key of the user.
        /// For update operations, this value identifies the user to update.
        /// </summary>
        public Guid UserPK { get; set; }

        /// <summary>
        /// Login user name to create or update.
        /// </summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// User email address.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Plain-text password supplied on create/update.
        /// The service is responsible for hashing it.
        /// </summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Whether the account should be active.
        /// </summary>
        public bool ActiveStatus { get; set; }

        /// <summary>
        /// Role type primary key to assign to the user.
        /// </summary>
        public Guid RoleTypePK { get; set; }

        /// <summary>
        /// User first name.
        /// </summary>
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// User last name (optional).
        /// </summary>
        public string? LastName { get; set; }

        /// <summary>
        /// User birth date (optional).
        /// </summary>
        public DateTime? BirthDate { get; set; }
    }
}
