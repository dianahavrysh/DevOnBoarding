using Common.Database;
using System;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace DataLayer.MsSql {
    /// <summary>
    /// SQL Server implementation of <see cref="Common.Database.Database"/>.
    /// Only supplies provider-specific primitives; the shared execution
    /// algorithm lives in the base class.
    /// </summary>
    public class MsSqlDatabase : Database {
        public MsSqlDatabase(string connectionString) : base(connectionString) {
        }

        /// <inheritdoc/>
        public override IDbConnection CreateConnection() {
            return new SqlConnection(_connectionString);
        }

        /// <inheritdoc/>
        public override IDataParameter CreateParameter(string name, object? value, System.Data.DbType? type = null) {
            var paramName = name.StartsWith("@") ? name : "@" + name;
            var p = new SqlParameter(paramName, value ?? DBNull.Value);
            if (type.HasValue)
                p.DbType = type.Value;
            return p;
        }

        /// <inheritdoc/>
        protected override async Task OpenAsync(IDbConnection connection) {
            await ((SqlConnection)connection).OpenAsync().ConfigureAwait(false);
        }

        /// <inheritdoc/>
        protected override async Task<IDataReader> ExecuteReaderCoreAsync(IDbCommand command) {
            return await ((SqlCommand)command).ExecuteReaderAsync(CommandBehavior.CloseConnection).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        protected override async Task<int> ExecuteNonQueryCoreAsync(IDbCommand command) {
            return await ((SqlCommand)command).ExecuteNonQueryAsync().ConfigureAwait(false);
        }

        /// <inheritdoc/>
        protected override async Task<object?> ExecuteScalarCoreAsync(IDbCommand command) {
            return await ((SqlCommand)command).ExecuteScalarAsync().ConfigureAwait(false);
        }
    }
}
