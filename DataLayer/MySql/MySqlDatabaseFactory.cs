using Common.Interfaces;
using Common.Database;

namespace DataLayer.MySql
{
    /// <summary>
    /// Factory that creates <see cref="MySqlDatabase"/> instances for MySQL.
    /// </summary>
    public class MySqlDatabaseFactory : IDatabaseFactory
    {
        private readonly string _connectionString;

        /// <summary>
        /// Initializes a new instance of <see cref="MySqlDatabaseFactory"/> with the provided connection string.
        /// </summary>
        /// <param name="connectionString">MySQL connection string.</param>
        public MySqlDatabaseFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <inheritdoc/>
        public Common.Database.Database CreateDatabase()
        {
            return new MySqlDatabase(_connectionString);
        }
    }
}
