using Common.Database;

namespace Common.Interfaces
{
    /// <summary>
    /// Provides a factory for creating provider-specific <see cref="Common.Database.Database"/> instances.
    /// Implementations return concrete database objects for MSSQL/MySQL providers.
    /// </summary>
    public interface IDatabaseFactory
    {
        /// <summary>
        /// Create a new database instance. The caller is responsible for disposing the returned instance when finished.
        /// </summary>
        /// <returns>A new <see cref="Common.Database.Database"/> instance.</returns>
        Common.Database.Database CreateDatabase();
    }
}
