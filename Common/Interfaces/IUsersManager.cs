using Common.Entities;
using System;
using System.Collections.Generic;

namespace Common.Interfaces
{
    /// <summary>
    /// Manager interface for user operations.
    /// </summary>
    public interface IUsersManager
    {
        User? GetByPK(Guid userPK);

        IEnumerable<User> GetByPage(Guid requestingUserPK, int currentPage, int pageSize, string? sortExpression, string? searchValue, Dictionary<string, bool>? searchByFields, bool includeInactive, bool strictMatch, out int totalRows);

        Guid Insert(User user);

        void Update(User user);

        void Delete(Guid userPK);
    }
}
