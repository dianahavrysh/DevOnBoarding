using Common;
using System.Data;
using Microsoft.Data.SqlClient;

namespace DataLayer.MsSql
{
    public class MsSqlDatabase : Database
    {
        public MsSqlDatabase(string connectionString) : base(connectionString)
        {
        }

        public override IDbConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }

        public override IDbCommand CreateCommand(string commandText, IDbConnection connection, CommandType commandType = CommandType.StoredProcedure)
        {
            var cmd = connection.CreateCommand();
            cmd.CommandText = commandText;
            cmd.CommandType = commandType;
            return cmd;
        }

        public override IDataParameter CreateParameter(string name, object? value, System.Data.DbType? type = null)
        {
            var p = new SqlParameter(name, value ?? DBNull.Value);
            if (type.HasValue)
                p.DbType = type.Value;
            return p;
        }
    }
}
