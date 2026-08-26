using System;
using Common.Database;
using System.Data;
using MySql.Data.MySqlClient;

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

        public override IDbCommand CreateCommand(string commandText, IDbConnection connection, CommandType commandType = CommandType.StoredProcedure)
        {
            var cmd = connection.CreateCommand();
            cmd.CommandText = commandText;
            cmd.CommandType = commandType;
            return cmd;
        }

        public override IDataParameter CreateParameter(string name, object? value, System.Data.DbType? type = null)
        {
            var p = new MySqlParameter(name, value ?? DBNull.Value);
            if (type.HasValue)
                p.DbType = type.Value;
            return p;
        }
    }
}
