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
    /// Database manager for user operations.
    /// </summary>
    public class UsersDbManager : BaseDbManager, IUsersManager {
        public UsersDbManager(
            IDatabaseFactory factory,
            ConnectionContext connectionContext)
            : base(factory, connectionContext) {
        }

        /// <inheritdoc />
        public async Task<Guid> InsertAsync(User user) {
            var parameters = new List<IDataParameter>
            {
                CreateParam(nameof(User.UserName), user.UserName),
                CreateParam(nameof(User.Email), user.Email),
                CreateParam(nameof(User.Password), user.Password),
                CreateParam(nameof(User.ActiveStatus), user.ActiveStatus),
                CreateParam(nameof(User.RoleTypePK), user.RoleTypePK),
                CreateParam(nameof(User.FirstName), user.FirstName),
                CreateParam(nameof(User.SecondName), user.SecondName),
                CreateParam(nameof(User.BirthDate), user.BirthDate)
            };

            var userPK = await ExecuteScalarAsync<Guid>(
                StoreProcedureNames.UsersInsert,
                parameters);

            if (userPK == Guid.Empty) {
                throw new InvalidOperationException(
                    "Users_INS did not return the created user's primary key.");
            }

            return userPK;
        }

        /// <inheritdoc />
        public async Task<bool> UpdateAsync(User user) {
            var found = CreateOutputParam(
                "Found",
                DbType.Boolean);

            var parameters = new List<IDataParameter>
            {
                CreateParam(nameof(User.UserPK), user.UserPK),
                CreateParam(nameof(User.UserName), user.UserName),
                CreateParam(nameof(User.Email), user.Email),
                CreateParam(nameof(User.Password), user.Password),
                CreateParam(nameof(User.ActiveStatus), user.ActiveStatus),
                CreateParam(nameof(User.RoleTypePK), user.RoleTypePK),
                CreateParam(nameof(User.FirstName), user.FirstName),
                CreateParam(nameof(User.SecondName), user.SecondName),
                CreateParam(nameof(User.BirthDate), user.BirthDate),
                found
            };

            await ExecuteNonQueryAsync(
                StoreProcedureNames.UsersUpdate,
                parameters);

            return found.Value is bool value && value;
        }

        /// <inheritdoc />
        public async Task DeleteAsync(Guid userPK) {
            var parameters = new List<IDataParameter>
            {
                CreateParam(nameof(User.UserPK), userPK)
            };

            await ExecuteNonQueryAsync(
                StoreProcedureNames.UsersDelete,
                parameters);
        }

        /// <inheritdoc />
        public async Task<User?> GetByPKAsync(Guid userPK) {
            var parameters = new List<IDataParameter>
            {
                CreateParam(nameof(User.UserPK), userPK)
            };

            using var reader = await ExecuteReaderAsync(
                StoreProcedureNames.UsersSelectByPK,
                parameters);

            return reader.Parse<User>().FirstOrDefault();
        }

        /// <inheritdoc />
        public async Task<(IEnumerable<User> Items, int TotalRows)> GetByPageAsync(
            Guid RequestingUserPK,
            int CurrentPage,
            int PageSize,
            string? SortExpression,
            string? SearchValue,
            Dictionary<string, bool>? SearchByFields,
            bool IncludeInactive,
            bool StrictMatch) {
            var parameters = new List<IDataParameter>
            {
                CreateParam(nameof(RequestingUserPK), RequestingUserPK),
                CreateParam(nameof(CurrentPage), CurrentPage),
                CreateParam(nameof(PageSize), PageSize),
                CreateParam(nameof(SortExpression), SortExpression),
                CreateParam(nameof(SearchValue), SearchValue),
                CreateParam(nameof(IncludeInactive), IncludeInactive),
                CreateParam(nameof(StrictMatch), StrictMatch)
            };

            AddSearchByFieldParameters(
                parameters,
                SearchByFields);

            using var reader = await ExecuteReaderAsync(
                StoreProcedureNames.UsersSelectByPage,
                parameters);

            var rows = reader.Parse<UserPageRow>().ToList();
            var totalRows = rows.FirstOrDefault()?.TotalRows ?? 0;

            return (
                rows.Cast<User>(),
                totalRows);
        }

        private void AddSearchByFieldParameters(
            List<IDataParameter> parameters,
            Dictionary<string, bool>? searchByFields) {
            if (searchByFields == null) {
                return;
            }

            foreach (var field in searchByFields) {
                parameters.Add(
                    CreateParam(
                        $"SearchBy{field.Key}",
                        field.Value));
            }
        }

        private sealed class UserPageRow : User {
            public int TotalRows { get; set; }
        }
    }
}
