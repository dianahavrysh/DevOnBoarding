using System;
using Common.Database;
using System.Data;
using Microsoft.Data.SqlClient;

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
        public override IDbCommand CreateCommand(string commandText, IDbConnection connection, CommandType commandType = CommandType.StoredProcedure)
        {
            var cmd = connection.CreateCommand();
            cmd.CommandText = commandText;
            cmd.CommandType = commandType;
            return cmd;
        }

        /// <inheritdoc/>
        public override IDataParameter CreateParameter(string name, object? value, System.Data.DbType? type = null)
        {
            var p = new SqlParameter(name, value ?? DBNull.Value);
            if (type.HasValue)
                p.DbType = type.Value;
            return p;
        }
    }
}
