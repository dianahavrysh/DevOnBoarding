using Common.Interfaces;
using Common.Database;

namespace DataLayer.MySql
{
    public class MySqlDatabaseFactory : IDatabaseFactory
    {
        private readonly string _connectionString;

        public MySqlDatabaseFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        public Database CreateDatabase()
        {
            return new MySqlDatabase(_connectionString);
        }
    }
}
