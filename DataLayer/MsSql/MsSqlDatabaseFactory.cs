using Common.Interfaces;
using Common.Database;

namespace DataLayer.MsSql
{
    /// <summary>
    /// Factory that creates <see cref="MsSqlDatabase"/> instances for SQL Server.
    /// </summary>
    public class MsSqlDatabaseFactory : IDatabaseFactory
    {
        private readonly string _connectionString;

        /// <summary>
        /// Initializes a new instance of <see cref="MsSqlDatabaseFactory"/> with the provided connection string.
        /// </summary>
        /// <param name="connectionString">SQL Server connection string.</param>
        public MsSqlDatabaseFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <inheritdoc/>
        public Common.Database.Database CreateDatabase()
        {
            return new MsSqlDatabase(_connectionString);
        }
    }
}
