using Common.Entities;
using Common.Interfaces;
using Common.Services.DTOs;
using System;
using System.Collections.Generic;

namespace Services
{
    public class UsersService : IUsersService
    {
        private readonly IUsersManager _manager;

        public UsersService(IUsersManager manager)
        {
            _manager = manager;
        }

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

        public void Delete(Guid userPK)
        {
            _manager.Delete(userPK);
        }

        public UserDTO? GetByPK(Guid userPK)
        {
            var u = _manager.GetByPK(userPK);
            if (u == null) return null;
            return MapToDTO(u);
        }

        public IEnumerable<UserDTO> GetByPage(Guid requestingUserPK, int currentPage, int pageSize, string? sortExpression, string? searchValue, Dictionary<string, bool>? searchByFields, bool includeInactive, bool strictMatch, out int totalRows)
        {
            var users = _manager.GetByPage(requestingUserPK, currentPage, pageSize, sortExpression, searchValue, searchByFields, includeInactive, strictMatch, out totalRows);
            var list = new List<UserDTO>();
            foreach (var u in users)
                list.Add(MapToDTO(u));
            return list;
        }

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

