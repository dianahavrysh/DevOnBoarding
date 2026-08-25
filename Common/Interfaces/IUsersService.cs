using Common.Entities;
using Common.Services.DTOs;
using System;
using System.Collections.Generic;

namespace Common.Interfaces
{
    /// <summary>
    /// Service interface producing DTOs for client consumption.
    /// </summary>
    public interface IUsersService
    {
        UserDTO? GetByPK(Guid userPK);

        IEnumerable<UserDTO> GetByPage(Guid requestingUserPK, int currentPage, int pageSize, string? sortExpression, string? searchValue, Dictionary<string, bool>? searchByFields, bool includeInactive, bool strictMatch, out int totalRows);

        Guid Create(UserCreateUpdateDTO dto);

        void Update(Guid userPK, UserCreateUpdateDTO dto);

        void Delete(Guid userPK);
    }
}
