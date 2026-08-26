using Common.Database;
using System.Data;
using MySql.Data.MySqlClient;
using System.Threading.Tasks;
using System;

namespace DataLayer.MySql
{
    public class MySqlDatabase : Database
    {
        public MySqlDatabase(string connectionString) : base(connectionString)
        {
        }

        public override IDbConnection CreateConnection()
        {
            return new MySqlConnection(_connectionString);
        }

        public override IDataParameter CreateParameter(string name, object? value, System.Data.DbType? type = null)
        {
            var paramName = name.StartsWith("@") ? name : "@" + name;
            var p = new MySqlParameter(paramName, value ?? DBNull.Value);
            if (type.HasValue)
                p.DbType = type.Value;
            return p;
        }

        public override async Task<IDataReader> ExecuteReaderAsync(string commandText, CommandType commandType = CommandType.StoredProcedure, IEnumerable<IDataParameter>? parameters = null)
        {
            var conn = (MySqlConnection)CreateConnection();
            var cmd = (MySqlCommand)CreateCommand(commandText, conn, commandType);
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

        public override async Task<int> ExecuteNonQueryAsync(string commandText, CommandType commandType = CommandType.StoredProcedure, IEnumerable<IDataParameter>? parameters = null)
        {
            await using var conn = (MySqlConnection)CreateConnection();
            await conn.OpenAsync().ConfigureAwait(false);
            await using var cmd = (MySqlCommand)CreateCommand(commandText, conn, commandType);
            if (parameters != null)
            {
                foreach (var p in parameters)
                    cmd.Parameters.Add(p);
            }

            return await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
        }

        public override async Task<object?> ExecuteScalarAsync(string commandText, CommandType commandType = CommandType.StoredProcedure, IEnumerable<IDataParameter>? parameters = null)
        {
            await using var conn = (MySqlConnection)CreateConnection();
            await conn.OpenAsync().ConfigureAwait(false);
            await using var cmd = (MySqlCommand)CreateCommand(commandText, conn, commandType);
            if (parameters != null)
            {
                foreach (var p in parameters)
                    cmd.Parameters.Add(p);
            }

            return await cmd.ExecuteScalarAsync().ConfigureAwait(false);
        }
    }
}
