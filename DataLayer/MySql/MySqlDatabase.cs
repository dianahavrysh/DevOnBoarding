using Common.Database;
using System;
using System.Data;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Data.Common;

namespace DataLayer.MySql {
    /// <summary>
    /// MySQL implementation of the data access <see cref="Database"/> abstraction.
    /// Only supplies provider-specific primitives; the shared execution
    /// algorithm lives in the base class.
    /// </summary>
    public class MySqlDatabase : Database {
        /// <summary>
        /// Initializes a new instance of the <see cref="MySqlDatabase"/> class.
        /// </summary>
        /// <param name="connectionString">Connection string for the MySQL server.</param>
        public MySqlDatabase(string connectionString) : base(connectionString) {
        }

        /// <inheritdoc/>
        public override IDbConnection CreateConnection() {
            return new MySqlConnection(_connectionString);
        }

        /// <inheritdoc/>
        public override IDataParameter CreateParameter(
            string name,
            object? value,
            System.Data.DbType? type = null) {
            var paramName = name.StartsWith("@") ? name : "@" + name;
            var p = new MySqlParameter(paramName, value ?? DBNull.Value);

            if (type.HasValue)
                p.DbType = type.Value;

            return p;
        }

        /// <inheritdoc/>
        public override IDataParameter CreateOutputParameter(
            string name,
            System.Data.DbType type) {
            var paramName = name.StartsWith("@") ? name : "@" + name;

            return new MySqlParameter(paramName, type) {
                Direction = ParameterDirection.Output
            };
        }

        /// <inheritdoc/>
        protected override async Task OpenAsync(IDbConnection connection) {
            await ((MySqlConnection)connection).OpenAsync();
        }

        /// <inheritdoc/>
        protected override async Task<DbDataReader> ExecuteReaderCoreAsync(
            IDbCommand command) {
            return await ((MySqlCommand)command)
                .ExecuteReaderAsync(CommandBehavior.CloseConnection);
        }

        /// <inheritdoc/>
        protected override async Task<int> ExecuteNonQueryCoreAsync(
            IDbCommand command) {
            return await ((MySqlCommand)command)
                .ExecuteNonQueryAsync();
        }

        /// <inheritdoc/>
        protected override async Task<object?> ExecuteScalarCoreAsync(
            IDbCommand command) {
            return await ((MySqlCommand)command)
                .ExecuteScalarAsync();
        }
    }
}
