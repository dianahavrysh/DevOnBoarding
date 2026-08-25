using Common.Entities;
using Common.Interfaces;
using Common.Services.DTOs;
using System;
using System.Collections.Generic;

namespace Services
{
    /// <summary>
    /// Application service that exposes user-related operations using DTOs for client consumption.
    /// Maps between domain entities and DTOs and delegates persistence to the users manager.
    /// </summary>
    public class UsersService : IUsersService
    {
        private readonly IUsersManager _manager;

        /// <summary>
        /// Initializes a new instance of <see cref="UsersService"/>.
        /// </summary>
        /// <param name="manager">Business logic manager used for persistence and retrieval.</param>
        public UsersService(IUsersManager manager)
        {
            _manager = manager;
        }

        /// <summary>
        /// Create a new user from the supplied DTO.
        /// </summary>
        /// <param name="dto">Write DTO containing user fields to create.</param>
        /// <returns>The primary key of the created user.</returns>
        public Guid Create(UserCreateUpdateDTO dto)
        {
            
            var user = new User
            {
                UserPK = Guid.NewGuid(),
                UserName = dto.UserName,
                Email = dto.Email,
                Password = dto.Password,
                ActiveStatus = dto.ActiveStatus,
                RoleTypePK = dto.RoleTypePK,
                RoleName = string.Empty,
                FirstName = dto.FirstName,
                SecondName = dto.LastName,
                BirthDate = dto.BirthDate
            };

            return _manager.Insert(user);
        }

        /// <summary>
        /// Delete the user identified by the specified primary key.
        /// </summary>
        /// <param name="userPK">Primary key of the user to delete.</param>
        public void Delete(Guid userPK)
        {
            _manager.Delete(userPK);
        }

        /// <summary>
        /// Get a user DTO by primary key.
        /// </summary>
        /// <param name="userPK">User primary key.</param>
        /// <returns>A <see cref="UserDTO"/> if the user exists; otherwise <c>null</c>.</returns>
        public UserDTO? GetByPK(Guid userPK)
        {
            var u = _manager.GetByPK(userPK);
            if (u == null) return null;
            return MapToDTO(u);
        }

        /// <summary>
        /// Retrieve a paged list of user DTOs.
        /// </summary>
        /// <inheritdoc cref="IUsersService.GetByPage(Guid,int,int,string?,string?,Dictionary{string,bool}?,bool,bool,out int)"/>
        public IEnumerable<UserDTO> GetByPage(Guid requestingUserPK, int currentPage, int pageSize, string? sortExpression, string? searchValue, Dictionary<string, bool>? searchByFields, bool includeInactive, bool strictMatch, out int totalRows)
        {
            var users = _manager.GetByPage(requestingUserPK, currentPage, pageSize, sortExpression, searchValue, searchByFields, includeInactive, strictMatch, out totalRows);
            var list = new List<UserDTO>();
            foreach (var u in users)
                list.Add(MapToDTO(u));
            return list;
        }

        /// <summary>
        /// Update an existing user with values from the write DTO.
        /// </summary>
        /// <param name="userPK">Primary key of the user to update.</param>
        /// <param name="dto">Write DTO with updated values.</param>
        public void Update(Guid userPK, UserCreateUpdateDTO dto)
        {
            var existing = _manager.GetByPK(userPK);
            if (existing == null) throw new ArgumentException("User not found", nameof(userPK));

            existing.UserName = dto.UserName;
            existing.Email = dto.Email;
            existing.Password = dto.Password;
            existing.ActiveStatus = dto.ActiveStatus;
            existing.RoleTypePK = dto.RoleTypePK;
            // RoleName is not provided in write DTO; keep existing or set empty
            existing.FirstName = dto.FirstName;
            existing.SecondName = dto.LastName;
            existing.BirthDate = dto.BirthDate;

            _manager.Update(existing);
        }

        private UserDTO MapToDTO(User u)
        {
            return new UserDTO
            {
                UserPK = u.UserPK,
                UserName = u.UserName,
                Email = u.Email,
                ActiveStatus = u.ActiveStatus,
                RoleName = u.RoleName,
                FirstName = u.FirstName,
                LastName = u.SecondName,
                BirthDate = u.BirthDate
            };
        }
    }
}

