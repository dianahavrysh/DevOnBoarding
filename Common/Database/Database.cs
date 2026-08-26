using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Common.Database
{
    /// <summary>
    /// Abstract base class for provider-specific database implementations.
    /// Concrete providers must implement connection and parameter creation and the provider-specific async execution primitives.
    /// </summary>
    public abstract class Database
    {
        protected readonly string _connectionString;

        protected Database(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Create a new <see cref="IDbConnection"/> for the underlying provider.
        /// </summary>
        /// <returns>A closed <see cref="IDbConnection"/> instance ready to be opened.</returns>
        public abstract IDbConnection CreateConnection();

        /// <summary>
        /// Default implementation that creates a command for the specified connection.
        /// Providers may override if they need custom command initialization.
        /// </summary>
        public virtual IDbCommand CreateCommand(string commandText, IDbConnection connection, CommandType commandType = CommandType.StoredProcedure)
        {
            var cmd = connection.CreateCommand();
            cmd.CommandText = commandText;
            cmd.CommandType = commandType;
            return cmd;
        }

        /// <summary>
        /// Create a provider-specific parameter for commands.
        /// </summary>
        /// <param name="name">Parameter name (provider prefix is applied by implementations when needed).</param>
        /// <param name="value">Parameter value or <c>null</c> to represent <see cref="DBNull.Value"/>.</param>
        /// <param name="type">Optional <see cref="DbType"/> for the parameter.</param>
        /// <returns>An <see cref="IDataParameter"/> instance.</returns>
        public abstract IDataParameter CreateParameter(string name, object? value, System.Data.DbType? type = null);

        /// <summary>
        /// Execute a command that returns a data reader asynchronously.
        /// The caller is responsible for disposing the returned <see cref="IDataReader"/>; the underlying connection will be closed when the reader is disposed.
        /// </summary>
        public abstract Task<IDataReader> ExecuteReaderAsync(string commandText, CommandType commandType = CommandType.StoredProcedure, IEnumerable<IDataParameter>? parameters = null);

        /// <summary>
        /// Execute a command that does not return rows (INSERT/UPDATE/DELETE) asynchronously.
        /// </summary>
        public abstract Task<int> ExecuteNonQueryAsync(string commandText, CommandType commandType = CommandType.StoredProcedure, IEnumerable<IDataParameter>? parameters = null);

        /// <summary>
        /// Execute a command and return the first column of the first row in the result set asynchronously.
        /// </summary>
        public abstract Task<object?> ExecuteScalarAsync(string commandText, CommandType commandType = CommandType.StoredProcedure, IEnumerable<IDataParameter>? parameters = null);
    }
}

