using Common.Database;
using Common.Interfaces;
using Common;
using System;
using System.Data;

namespace BusinessLogic
{
    /// <summary>
    /// Base class for database-backed business managers. Provides access to the configured <see cref="Database"/> instance
    /// and helper methods for creating parameters.
    /// </summary>
    public abstract class BaseDbManager
    {
        /// <summary>
        /// The provider-specific database instance used to execute commands.
        /// </summary>
        protected readonly Database Db;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseDbManager"/> class.
        /// </summary>
        /// <param name="factory">Factory used to create the database instance.</param>
        /// <param name="connectionContext">Per-request connection context (DB type + connection string).</param>
        protected BaseDbManager(IDatabaseFactory factory, ConnectionContext connectionContext)
        {
            Db = factory.CreateDatabase(connectionContext);
        }

        /// <summary>
        /// Create a database parameter with the given name and value.
        /// </summary>
        /// <param name="name">Parameter name (without <c>@</c> prefix).</param>
        /// <param name="value">Parameter value; <c>null</c> is mapped to <see cref="DBNull.Value"/>.</param>
        /// <returns>A provider-specific <see cref="IDataParameter"/> instance.</returns>
        protected IDataParameter Param(string name, object? value)
        {
            return Db.CreateParameter(name, value ?? DBNull.Value);
        }
    }
}
