using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Common.Database {
    /// <summary>
    /// Abstract base class for provider-specific database implementations.
    /// Implements the shared execution algorithm (open connection → create command →
    /// add parameters → execute → return) via the Template Method pattern.
    /// Concrete providers only need to supply the provider-specific primitives
    /// (connection/parameter creation and the low-level async execution calls).
    /// </summary>
    public abstract class Database {
        /// <summary>
        /// The connection string used by provider implementations to create connections.
        /// Protected so derived classes can use it when constructing provider objects.
        /// </summary>
        protected readonly string _connectionString;

        /// <summary>
        /// Initializes the database base with the provided provider-specific connection string.
        /// </summary>
        /// <param name="connectionString">Provider connection string.</param>
        protected Database(string connectionString) {
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
        public virtual IDbCommand CreateCommand(string commandText, IDbConnection connection, CommandType commandType = CommandType.StoredProcedure) {
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
        public virtual async Task<IDataReader> ExecuteReaderAsync(string commandText, CommandType commandType = CommandType.StoredProcedure, IEnumerable<IDataParameter>? parameters = null) {
            var connection = CreateConnection();
            var command = CreateCommand(commandText, connection, commandType);
            AddParameters(command, parameters);

            try {
                await OpenAsync(connection).ConfigureAwait(false);
                return await ExecuteReaderCoreAsync(command).ConfigureAwait(false);
            }
            catch {
                connection.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Execute a command that does not return rows (INSERT/UPDATE/DELETE) asynchronously.
        /// </summary>
        public virtual async Task<int> ExecuteNonQueryAsync(string commandText, CommandType commandType = CommandType.StoredProcedure, IEnumerable<IDataParameter>? parameters = null) {
            using var connection = CreateConnection();
            await OpenAsync(connection).ConfigureAwait(false);
            using var command = CreateCommand(commandText, connection, commandType);
            AddParameters(command, parameters);

            return await ExecuteNonQueryCoreAsync(command).ConfigureAwait(false);
        }

        /// <summary>
        /// Execute a command and return the first column of the first row in the result set asynchronously.
        /// </summary>
        public virtual async Task<object?> ExecuteScalarAsync(string commandText, CommandType commandType = CommandType.StoredProcedure, IEnumerable<IDataParameter>? parameters = null) {
            using var connection = CreateConnection();
            await OpenAsync(connection).ConfigureAwait(false);
            using var command = CreateCommand(commandText, connection, commandType);
            AddParameters(command, parameters);

            return await ExecuteScalarCoreAsync(command).ConfigureAwait(false);
        }

        /// <summary>
        /// Execute a scalar command and parse the result as a Guid. Returns Guid.Empty when the result is null, DBNull.Value or not parsable.
        /// </summary>
        public virtual async Task<Guid> ExecuteScalarGuidAsync(string commandText, CommandType commandType = CommandType.StoredProcedure, IEnumerable<IDataParameter>? parameters = null) {
            var obj = await ExecuteScalarAsync(commandText, commandType, parameters).ConfigureAwait(false);
            if (obj == null || obj == DBNull.Value) return Guid.Empty;
            return Guid.TryParse(obj.ToString(), out var g) ? g : Guid.Empty;
        }

        /// <summary>
        /// Adds the supplied parameters to the command. Shared helper for all execution paths.
        /// </summary>
        private static void AddParameters(IDbCommand command, IEnumerable<IDataParameter>? parameters) {
            if (parameters == null) return;

            foreach (var parameter in parameters)
                command.Parameters.Add(parameter);
        }

        /// <summary>
        /// Opens the given connection asynchronously using the provider-specific API.
        /// </summary>
        protected abstract Task OpenAsync(IDbConnection connection);

        /// <summary>
        /// Executes the given command as a reader asynchronously using the provider-specific API.
        /// Implementations should use <see cref="CommandBehavior.CloseConnection"/> so the connection
        /// is closed automatically when the returned reader is disposed.
        /// </summary>
        protected abstract Task<IDataReader> ExecuteReaderCoreAsync(IDbCommand command);

        /// <summary>
        /// Executes the given command as a non-query asynchronously using the provider-specific API.
        /// </summary>
        protected abstract Task<int> ExecuteNonQueryCoreAsync(IDbCommand command);

        /// <summary>
        /// Executes the given command as a scalar asynchronously using the provider-specific API.
        /// </summary>
        protected abstract Task<object?> ExecuteScalarCoreAsync(IDbCommand command);
    }
}
