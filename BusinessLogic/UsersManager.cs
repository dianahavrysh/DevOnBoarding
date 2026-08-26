using Common.Entities;
using Common.Interfaces;
using Common.Database;
using Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using BusinessLogic.Mappers;

namespace BusinessLogic {
    /// <summary>
    /// Business logic implementation for user operations. This class delegates to the data layer for persistence.
    /// </summary>
    public class UsersManager : BaseDbManager, IUsersManager {

        /// <summary>
        /// Initializes a new instance of the <see cref="UsersManager"/> class.
        /// </summary>
        /// <param name="factory">Database factory used to create provider-specific database instances.</param>
        public UsersManager(IDatabaseFactory factory) : base(factory) {
        }

        /// <inheritdoc />
        public async Task<Guid> InsertAsync(User user) {
            var parameters = new List<IDataParameter>
            {
                Param("UserName", user.UserName),
                Param("Email", user.Email),
                Param("Password", user.Password),
                Param("ActiveStatus", user.ActiveStatus),
                Param("RoleTypePK", user.RoleTypePK),
                Param("FirstName", user.FirstName),
                Param("SecondName", user.SecondName ?? (object)DBNull.Value),
                Param("BirthDate", user.BirthDate ?? (object)DBNull.Value)
            };

            var obj = await Db.ExecuteScalarAsync(StoreProcedureNames.UsersInsert, CommandType.StoredProcedure, parameters).ConfigureAwait(false);

            if (obj == null || obj == DBNull.Value) return Guid.Empty;
            return Guid.TryParse(obj.ToString(), out var g) ? g : Guid.Empty;
        }

        /// <inheritdoc />
        public async Task UpdateAsync(User user) {
            var parameters = new List<IDataParameter>
            {
                Param("UserPK", user.UserPK),
                Param("UserName", user.UserName),
                Param("Email", user.Email),
                Param("Password", user.Password),
                Param("ActiveStatus", user.ActiveStatus),
                Param("RoleTypePK", user.RoleTypePK),
                Param("FirstName", user.FirstName),
                Param("SecondName", user.SecondName ?? (object)DBNull.Value),
                Param("BirthDate", user.BirthDate ?? (object)DBNull.Value)
            };

            await Db.ExecuteNonQueryAsync(StoreProcedureNames.UsersUpdate, CommandType.StoredProcedure, parameters).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task DeleteAsync(Guid userPK) {
            var parameters = new List<IDataParameter>
            {
                Param("UserPK", userPK)
            };

            await Db.ExecuteNonQueryAsync(StoreProcedureNames.UsersDelete, CommandType.StoredProcedure, parameters).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<User?> GetByPKAsync(Guid userPK) {
            var parameters = new List<IDataParameter>
            {
                Param("UserPK", userPK)
            };

            using var reader = await Db.ExecuteReaderAsync(StoreProcedureNames.UsersSelectByPK, CommandType.StoredProcedure, parameters).ConfigureAwait(false);
            var dbReader = (System.Data.Common.DbDataReader)reader;

            if (await dbReader.ReadAsync().ConfigureAwait(false)) {
                var user = UserReaderMapper.Map(reader);
                return user;
            }

            return null;
        }

        /// <inheritdoc />
        public async Task<(IEnumerable<User> Items, int TotalRows)> GetByPageAsync(
            Guid requestingUserPK,
            int currentPage,
            int pageSize,
            string? sortExpression,
            string? searchValue,
            Dictionary<string, bool>? searchByFields,
            bool includeInactive,
            bool strictMatch) {
            var parameters = new List<IDataParameter>
            {
                Param("RequestingUserPK", requestingUserPK),
                Param("CurrentPage", currentPage),
                Param("PageSize", pageSize),
                Param("SortExpression", sortExpression ?? string.Empty),
                Param("SearchValue", searchValue ?? string.Empty),
                Param("IncludeInactive", includeInactive),
                Param("StrictMatch", strictMatch)
            };

            AddSearchByFieldParameters(parameters, searchByFields);

            using var reader = await Db.ExecuteReaderAsync(StoreProcedureNames.UsersSelectByPage, CommandType.StoredProcedure, parameters).ConfigureAwait(false);
            var dbReader = (System.Data.Common.DbDataReader)reader;

            var list = new List<User>();
            while (await dbReader.ReadAsync().ConfigureAwait(false)) {
                list.Add(UserReaderMapper.Map(reader));
            }

            var totalRows = 0;
            try {
                if (await dbReader.NextResultAsync().ConfigureAwait(false) && await dbReader.ReadAsync().ConfigureAwait(false)) {
                    totalRows = dbReader.GetValue<int>("TotalRows");
                }
            }
            catch {
                // ignored
            }

            return (list, totalRows);
        }

        private void AddSearchByFieldParameters(List<IDataParameter> parameters, Dictionary<string, bool>? searchByFields) {
            if (searchByFields == null) return;

            foreach (var field in searchByFields) {
                parameters.Add(Param($"SearchBy{field.Key}", field.Value));
            }
        }
    }
}
