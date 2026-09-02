using Common;
using Common.Database;
using Common.Extensions;
using Common.Entities;
using Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace DAL {
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
            var newUserPK = CreateOutputParam(
                "NewUserPK",
                DbType.Guid);

            var parameters = BuildUserParameters(user, includeUserPK: false);
            parameters.Add(newUserPK);

            await ExecuteNonQueryAsync(
                StoreProcedureNames.UsersInsert,
                parameters);

            if (newUserPK.Value is not Guid userPK || userPK == Guid.Empty) {
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

            var parameters = BuildUserParameters(user, includeUserPK: true);
            parameters.Add(found);

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
            var user = new User();

            var parameters = new List<IDataParameter>
            {
                CreateParam(nameof(User.UserPK), userPK)
            };

            using var reader = await ExecuteReaderAsync(
                StoreProcedureNames.UsersSelectByPK,
                parameters);

            var dbReader = (DbDataReader)reader;

            if (await dbReader.ReadAsync()) {
                user = PopulateUser(dbReader);
            }

            return user;
        }

        /// <inheritdoc />
        public async Task<(List<User> Items, int TotalRows)> GetByPageAsync(
            Guid RequestingUserPK,
            int CurrentPage,
            int PageSize,
            string? SortExpression,
            string? SearchValue,
            Dictionary<string, bool>? SearchByFields,
            bool IncludeInactive,
            bool StrictMatch) {
            var users = new List<User>();
            var totalRows = 0;

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

            AddSearchByFieldParameters(parameters, SearchByFields);

            using var reader = await ExecuteReaderAsync(
                StoreProcedureNames.UsersSelectByPage,
                parameters);

            var dbReader = (DbDataReader)reader;

            if (await dbReader.ReadAsync()) {
                totalRows = dbReader.GetValue("TotalRows", 0);

                do {
                    users.Add(PopulateUser(dbReader));
                }
                while (await dbReader.ReadAsync());
            }

            return (users, totalRows);
        }

        /// <summary>
        /// Populates a <see cref="User"/> from the current row of the reader
        /// using the <see cref="DataReaderExtensions.GetValue{T}"/> helpers.
        /// Shared by <see cref="GetByPKAsync"/> and <see cref="GetByPageAsync"/>.
        /// </summary>
        private static User PopulateUser(IDataReader reader) {
            return new User {
                UserPK = reader.GetValue(nameof(User.UserPK), Guid.Empty),
                UserName = reader.GetValue(nameof(User.UserName)),
                Email = reader.GetValue(nameof(User.Email)),
                Password = reader.GetValue(nameof(User.Password)),
                ActiveStatus = reader.GetValue(nameof(User.ActiveStatus), false),
                RoleTypePK = reader.GetValue(nameof(User.RoleTypePK), Guid.Empty),
                FirstName = reader.GetValue(nameof(User.FirstName)),
                SecondName = reader.GetValue(nameof(User.SecondName)),
                BirthDate = reader.GetValue<DateTime?>(nameof(User.BirthDate), null)
            };
        }

        /// <summary>
        /// Builds the shared set of parameters used by both <see cref="InsertAsync"/> and
        /// <see cref="UpdateAsync"/>, to avoid repeating the same eight parameters twice.
        /// The output parameter (NewUserPK / Found) is added by the caller.
        /// </summary>
        private List<IDataParameter> BuildUserParameters(User user, bool includeUserPK) {
            var parameters = new List<IDataParameter>();

            if (includeUserPK) {
                parameters.Add(CreateParam(nameof(User.UserPK), user.UserPK));
            }

            parameters.Add(CreateParam(nameof(User.UserName), user.UserName));
            parameters.Add(CreateParam(nameof(User.Email), user.Email));
            parameters.Add(CreateParam(nameof(User.Password), user.Password));
            parameters.Add(CreateParam(nameof(User.ActiveStatus), user.ActiveStatus));
            parameters.Add(CreateParam(nameof(User.RoleTypePK), user.RoleTypePK));
            parameters.Add(CreateParam(nameof(User.FirstName), user.FirstName));
            parameters.Add(CreateParam(nameof(User.SecondName), user.SecondName));
            parameters.Add(CreateParam(nameof(User.BirthDate), user.BirthDate));

            return parameters;
        }

        private void AddSearchByFieldParameters(
            ICollection<IDataParameter> parameters,
            Dictionary<string, bool>? searchByFields) {
            searchByFields ??= new Dictionary<string, bool>();

            parameters.Add(CreateParam(
                "SearchByUserName",
                searchByFields.GetValueOrDefault("UserName")));
            parameters.Add(CreateParam(
                "SearchByEmail",
                searchByFields.GetValueOrDefault("Email")));
            parameters.Add(CreateParam(
                "SearchByFirstName",
                searchByFields.GetValueOrDefault("FirstName")));
            parameters.Add(CreateParam(
                "SearchBySecondName",
                searchByFields.GetValueOrDefault("SecondName")));
        }
    }
}
