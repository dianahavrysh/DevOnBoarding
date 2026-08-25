using Common;
using Common.Entities;
using Common.Interfaces;
using Common.Database;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace BusinessLogic {
    public class UsersManager : IUsersManager {
        private readonly IDatabaseFactory _factory;

        public UsersManager(IDatabaseFactory factory) {
            _factory = factory;
        }

        public Guid Insert(User user) {
            using var db = _factory.CreateDatabase();

            var parameters = new List<IDataParameter>
            {
                db.CreateParameter("@UserName", user.UserName),
                db.CreateParameter("@Email", user.Email),
                db.CreateParameter("@Password", user.Password),
                db.CreateParameter("@ActiveStatus", user.ActiveStatus),
                db.CreateParameter("@RoleTypePK", user.RoleTypePK),
                db.CreateParameter("@FirstName", user.FirstName),
                db.CreateParameter("@SecondName", user.SecondName ?? (object)DBNull.Value),
                db.CreateParameter("@BirthDate", user.BirthDate ?? (object)DBNull.Value)
            };

            var obj = db.ExecuteScalarAsync("Users_INS", CommandType.StoredProcedure, parameters)
                         .GetAwaiter().GetResult();

            if (obj == null || obj == DBNull.Value) return Guid.Empty;
            return Guid.TryParse(obj.ToString(), out var g) ? g : Guid.Empty;
        }

        public void Update(User user) {
            using var db = _factory.CreateDatabase();

            var parameters = new List<IDataParameter>
            {
                db.CreateParameter("@UserPK", user.UserPK),
                db.CreateParameter("@UserName", user.UserName),
                db.CreateParameter("@Email", user.Email),
                db.CreateParameter("@Password", user.Password),
                db.CreateParameter("@ActiveStatus", user.ActiveStatus),
                db.CreateParameter("@RoleTypePK", user.RoleTypePK),
                db.CreateParameter("@FirstName", user.FirstName),
                db.CreateParameter("@SecondName", user.SecondName ?? (object)DBNull.Value),
                db.CreateParameter("@BirthDate", user.BirthDate ?? (object)DBNull.Value)
            };

            db.ExecuteNonQueryAsync("Users_UPD", CommandType.StoredProcedure, parameters)
              .GetAwaiter().GetResult();
        }

        public void Delete(Guid userPK) {
            using var db = _factory.CreateDatabase();

            var parameters = new List<IDataParameter>
            {
                db.CreateParameter("@UserPK", userPK)
            };

            db.ExecuteNonQueryAsync("Users_DEL", CommandType.StoredProcedure, parameters)
              .GetAwaiter().GetResult();
        }

        public User? GetByPK(Guid userPK) {
            using var db = _factory.CreateDatabase();

            var parameters = new List<IDataParameter>
            {
                db.CreateParameter("@UserPK", userPK)
            };

            using var reader = db.ExecuteReaderAsync("Users_SEL_ByPK", CommandType.StoredProcedure, parameters)
                                  .GetAwaiter().GetResult();

            if (reader.Read()) {
                var user = MapUser(reader);
                reader.Close();
                return user;
            }

            reader.Close();
            return null;
        }

        public IEnumerable<User> GetByPage(
            Guid requestingUserPK,
            int currentPage,
            int pageSize,
            string? sortExpression,
            string? searchValue,
            Dictionary<string, bool>? searchByFields,
            bool includeInactive,
            bool strictMatch,
            out int totalRows) {
            totalRows = 0;
            using var db = _factory.CreateDatabase();

            var parameters = new List<IDataParameter>
            {
                db.CreateParameter("@RequestingUserPK", requestingUserPK),
                db.CreateParameter("@CurrentPage", currentPage),
                db.CreateParameter("@PageSize", pageSize),
                db.CreateParameter("@SortExpression", sortExpression ?? string.Empty),
                db.CreateParameter("@SearchValue", searchValue ?? string.Empty),
                db.CreateParameter("@IncludeInactive", includeInactive),
                db.CreateParameter("@StrictMatch", strictMatch)
            };

            AddSearchByFieldParameters(db, parameters, searchByFields);

            using var reader = db.ExecuteReaderAsync("Users_SEL_ByPage", CommandType.StoredProcedure, parameters)
                                  .GetAwaiter().GetResult();

            var list = new List<User>();
            while (reader.Read()) {
                list.Add(MapUser(reader));
            }

            try {
                if (reader.NextResult() && reader.Read()) {
                    totalRows = reader.GetValue<int>("TotalRows");
                }
            }
            catch {
                // total rows not available from this result set; leave as 0
            }

            reader.Close();
            return list;
        }

        private static void AddSearchByFieldParameters(
            Database db,
            List<IDataParameter> parameters,
            Dictionary<string, bool>? searchByFields) {
            if (searchByFields == null) return;

            foreach (var field in searchByFields) {
                // expects keys like "UserName", "Email", "FirstName", "SecondName"
                // mapped to SP parameters "@SearchByUserName", "@SearchByEmail", etc.
                parameters.Add(db.CreateParameter($"@SearchBy{field.Key}", field.Value));
            }
        }

        private static User MapUser(IDataReader reader) {
            return new User {
                UserPK = reader.GetValue<Guid>("UserPK"),
                UserName = reader.GetValue<string>("UserName"),
                Email = reader.GetValue<string>("Email"),
                Password = reader.GetValue<string>("Password"),
                ActiveStatus = reader.GetValue<bool>("ActiveStatus"),
                RoleTypePK = reader.GetValue<Guid>("RoleTypePK"),
                RoleName = reader.GetValue<string>("RoleName"),
                FirstName = reader.GetValue<string>("FirstName"),
                SecondName = reader.GetValue<string?>("SecondName"),
                BirthDate = reader.GetValue<DateTime?>("BirthDate")
            };
        }
    }
}
