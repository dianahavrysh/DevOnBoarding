using Common.Database;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using System;
using System.Data.Common;

namespace DataLayer.MsSql
{
    /// <summary>
    /// SQL Server implementation of <see cref="Common.Database.Database"/>.
    /// </summary>
    public class MsSqlDatabase : Database
    {
        public MsSqlDatabase(string connectionString) : base(connectionString)
        {
        }

        /// <inheritdoc/>
        public override IDbConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }

        /// <inheritdoc/>
        public override IDataParameter CreateParameter(string name, object? value, System.Data.DbType? type = null)
        {
            var paramName = name.StartsWith("@") ? name : "@" + name;
            var p = new SqlParameter(paramName, value ?? DBNull.Value);
            if (type.HasValue)
                p.DbType = type.Value;
            return p;
        }

        /// <inheritdoc/>
        public override async Task<IDataReader> ExecuteReaderAsync(string commandText, CommandType commandType = CommandType.StoredProcedure, IEnumerable<IDataParameter>? parameters = null)
        {
            var conn = (SqlConnection)CreateConnection();
            var cmd = (SqlCommand)CreateCommand(commandText, conn, commandType);
            if (parameters != null)
            {
                foreach (var p in parameters)
                    cmd.Parameters.Add(p);
            }

            try
            {
                if (conn.State != ConnectionState.Open)
                    await conn.OpenAsync().ConfigureAwait(false);

                var reader = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection).ConfigureAwait(false);
                return reader;
            }
            catch
            {
                conn.Dispose();
                throw;
            }
        }

        /// <inheritdoc/>
        public override async Task<int> ExecuteNonQueryAsync(string commandText, CommandType commandType = CommandType.StoredProcedure, IEnumerable<IDataParameter>? parameters = null)
        {
            await using var conn = (SqlConnection)CreateConnection();
            await conn.OpenAsync().ConfigureAwait(false);
            await using var cmd = (SqlCommand)CreateCommand(commandText, conn, commandType);
            if (parameters != null)
            {
                foreach (var p in parameters)
                    cmd.Parameters.Add(p);
            }

            return await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public override async Task<object?> ExecuteScalarAsync(string commandText, CommandType commandType = CommandType.StoredProcedure, IEnumerable<IDataParameter>? parameters = null)
        {
            await using var conn = (SqlConnection)CreateConnection();
            await conn.OpenAsync().ConfigureAwait(false);
            await using var cmd = (SqlCommand)CreateCommand(commandText, conn, commandType);
            if (parameters != null)
            {
                foreach (var p in parameters)
                    cmd.Parameters.Add(p);
            }

            return await cmd.ExecuteScalarAsync().ConfigureAwait(false);
        }
    }
}
