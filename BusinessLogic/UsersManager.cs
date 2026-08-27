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
        /// <param name="connectionContext">Connection context for this manager lifetime (scoped per request/user).</param>
        public UsersManager(IDatabaseFactory factory, ConnectionContext connectionContext) : base(factory, connectionContext) {
        }

        /// <inheritdoc />
        public async Task<Guid> InsertAsync(User user) {
            var parameters = new List<IDataParameter>
            {
                Param(nameof(User.UserName), user.UserName),
                Param(nameof(User.Email), user.Email),
                Param(nameof(User.Password), user.Password),
                Param(nameof(User.ActiveStatus), user.ActiveStatus),
                Param(nameof(User.RoleTypePK), user.RoleTypePK),
                Param(nameof(User.FirstName), user.FirstName),
                Param(nameof(User.SecondName), user.SecondName ?? (object)DBNull.Value),
                Param(nameof(User.BirthDate), user.BirthDate ?? (object)DBNull.Value)
            };

            var g = await Db.ExecuteScalarGuidAsync(StoreProcedureNames.UsersInsert, parameters: parameters);
            return g;
        }

        /// <inheritdoc />
        public async Task UpdateAsync(User user) {
            var parameters = new List<IDataParameter>
            {
                Param(nameof(User.UserPK), user.UserPK),
                Param(nameof(User.UserName), user.UserName),
                Param(nameof(User.Email), user.Email),
                Param(nameof(User.Password), user.Password),
                Param(nameof(User.ActiveStatus), user.ActiveStatus),
                Param(nameof(User.RoleTypePK), user.RoleTypePK),
                Param(nameof(User.FirstName), user.FirstName),
                Param(nameof(User.SecondName), user.SecondName ?? (object)DBNull.Value),
                Param(nameof(User.BirthDate), user.BirthDate ?? (object)DBNull.Value)
            };

            await Db.ExecuteNonQueryAsync(StoreProcedureNames.UsersUpdate, parameters: parameters);
        }

        /// <inheritdoc />
        public async Task DeleteAsync(Guid userPK) {
            var parameters = new List<IDataParameter>
            {
                Param(nameof(User.UserPK), userPK)
            };

            await Db.ExecuteNonQueryAsync(StoreProcedureNames.UsersDelete, parameters: parameters);
        }

        /// <inheritdoc />
        public async Task<User?> GetByPKAsync(Guid userPK) {
            User? user = null;
            var parameters = new List<IDataParameter> { Param(nameof(User.UserPK), userPK) };

            using var reader = await Db.ExecuteReaderAsync(StoreProcedureNames.UsersSelectByPK, parameters: parameters);
            var dbReader = (System.Data.Common.DbDataReader)reader;

            if (await dbReader.ReadAsync()) {
                user = UserReaderMapper.Map(reader);
            }

            return user;
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

            using var reader = await Db.ExecuteReaderAsync(StoreProcedureNames.UsersSelectByPage, parameters: parameters);
            var dbReader = (System.Data.Common.DbDataReader)reader;

            var list = new List<User>();
            while (await dbReader.ReadAsync()) {
                list.Add(UserReaderMapper.Map(reader));
            }

            var totalRows = 0;
            try {
                if (await dbReader.NextResultAsync() && await dbReader.ReadAsync()) {
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
