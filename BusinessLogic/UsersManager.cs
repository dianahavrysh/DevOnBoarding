using Common;
using Common.Database;
using Common.Entities;
using Common.Interfaces;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace BusinessLogic {
    /// <summary>
    /// Business logic implementation for user operations.
    /// This class delegates to the data layer for persistence.
    /// </summary>
    public class UsersManager : BaseDbManager, IUsersManager {
        public UsersManager(
            IDatabaseFactory factory,
            ConnectionContext connectionContext)
            : base(factory, connectionContext) {
        }

        /// <inheritdoc />
        public async Task<Guid> InsertAsync(User user) {
            var newUserPK = Db.CreateOutputParameter(
                "NewUserPK",
                DbType.Guid);

            var parameters = new List<IDataParameter>
            {
                Param(nameof(User.UserName), user.UserName),
                Param(nameof(User.Email), user.Email),
                Param(nameof(User.Password), user.Password),
                Param(nameof(User.ActiveStatus), user.ActiveStatus),
                Param(nameof(User.RoleTypePK), user.RoleTypePK),
                Param(nameof(User.FirstName), user.FirstName),
                Param(nameof(User.SecondName), user.SecondName),
                Param(nameof(User.BirthDate), user.BirthDate),
                newUserPK
            };

            await Db.ExecuteNonQueryAsync(
                StoreProcedureNames.UsersInsert,
                parameters: parameters);

            return newUserPK.Value is Guid userPK
                ? userPK
                : Guid.Empty;
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
                Param(nameof(User.SecondName), user.SecondName),
                Param(nameof(User.BirthDate), user.BirthDate)
            };

            await Db.ExecuteNonQueryAsync(
                StoreProcedureNames.UsersUpdate,
                parameters: parameters);
        }

        /// <inheritdoc />
        public async Task DeleteAsync(Guid userPK) {
            var parameters = new List<IDataParameter>
            {
                Param(nameof(User.UserPK), userPK)
            };

            await Db.ExecuteNonQueryAsync(
                StoreProcedureNames.UsersDelete,
                parameters: parameters);
        }

        /// <inheritdoc />
        public async Task<User?> GetByPKAsync(Guid userPK) {
            var parameters = new List<IDataParameter>
            {
                Param(nameof(User.UserPK), userPK)
            };

            using var reader = await Db.ExecuteReaderAsync(
                StoreProcedureNames.UsersSelectByPK,
                parameters: parameters);

            return reader.Parse<User>().FirstOrDefault();
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

            using var reader = await Db.ExecuteReaderAsync(
                StoreProcedureNames.UsersSelectByPage,
                parameters: parameters);

            var dbReader = (System.Data.Common.DbDataReader)reader;

            var list = reader.Parse<User>().ToList();

            var totalRows = 0;

            try {
                if (await dbReader.NextResultAsync() &&
                    await dbReader.ReadAsync()) {
                    totalRows = dbReader.GetValue<int>("TotalRows");
                }
            }
            catch {
                // ignored
            }

            return (list, totalRows);
        }

        private void AddSearchByFieldParameters(
            List<IDataParameter> parameters,
            Dictionary<string, bool>? searchByFields) {
            if (searchByFields == null) {
                return;
            }

            foreach (var field in searchByFields) {
                parameters.Add(
                    Param($"SearchBy{field.Key}", field.Value));
            }
        }
    }
}
