using Common.Interfaces;
using Common;

namespace DataLayer.MsSql
{
    public class MsSqlDatabaseFactory : IDatabaseFactory
    {
        private readonly string _connectionString;

        public MsSqlDatabaseFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        public Database CreateDatabase()
        {
            return new MsSqlDatabase(_connectionString);
        }
    }
}
